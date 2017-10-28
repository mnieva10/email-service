unit uFileLogger;

interface

uses
  {$IFNDEF LIGHT_ASYNC_LOGGER}
  SvcBus, SvcBusMessage, SvcBusDispatcher, MongoBson, SvcBusResponder,
  {$ENDIF}
  SyncObjs, Classes,
  SysUtils, uLogFileStream;

const
  MAX_LOGFILES_PER_PROCESS = 100;

type
  {$IFDEF LIGHT_ASYNC_LOGGER}
  TFileLogLevel = (
    Always = 0,
    Fatal = 1,
    Error = 2,
    Warning = 3,
    Info = 4,
    Debug = 5
  );
  {$ENDIF}

  TFileLogger = class(TObject {$IFNDEF LIGHT_ASYNC_LOGGER}, ILogger {$ENDIF})
    FFile: TLogFileStream;
    FFlushEntriesBy: Cardinal;
    FLogLevel: TFileLogLevel;
    procedure WriteHeaderText;
    procedure WriteString(const s: string);
    function AppendBeforeExtension(const s, append: string): string;
    function GetPath: string;
  protected
    function QueryInterface(const IID: TGUID; out Obj): HResult; stdcall;
    function _AddRef: Integer; stdcall;
    function _Release: Integer; stdcall;
  public
    constructor Create(const ALogPath: string; ALogLevel: TFileLogLevel);
    destructor Destroy; override;
    procedure WriteLogEntry(ALogLevel: TFileLogLevel; const AMsg: string); overload;
    procedure WriteLogEntry(ALogLevel: TFileLogLevel; const AMsg: string; E : Exception); overload;
    procedure Flush; deprecated;
    property FlushEntriesBy: Cardinal read FFlushEntriesBy write FFlushEntriesBy default 10;
    property Path: string read GetPath;
    property LogLevel: TFileLogLevel read FLogLevel;
  end;

  {$IFNDEF LIGHT_ASYNC_LOGGER}
  TFileLogRequestStrategy = class(TChainableRequestProcessingStrategy)
    FLogger: TFileLogger;
  public
    constructor Create(const ALogger: TFileLogger);
    procedure Process(const AMsg: IMessage); override;
  end;

  TFileLogResponseStrategy = class(TInterfacedObject, IResponseProcessingStrategy)
    FLogger: TFileLogger;
  protected
    procedure ProcessResponse(response : IMessage);
  public
    constructor Create(const ALogger: TFileLogger);
  end;
  {$ENDIF}

  function FileLogLevelToStr(ALogLevel: TFileLogLevel): string;
  function StrToFileLogLevel(const ALogLevel: string): TFileLogLevel;

implementation

uses
  Windows;

const
  DATE_FORMAT = 'dd/mm hh:nn:ss.zzz';

{ TFileLogger }

function TFileLogger.AppendBeforeExtension(const s, append: string): string;
begin
  Result := Format('%s%s(%s)%s', [ExtractFilePath(s),
                                  ChangeFileExt(ExtractFileName(s), ''),
                                  append, ExtractFileExt(s)]);
end;

constructor TFileLogger.Create(const ALogPath: string; ALogLevel: TFileLogLevel);
var
  i: Cardinal;
  logPath: string;
  handle: THandle;
begin
  logPath := ALogPath;
  i := 1;
  while True do
    try
      handle := Integer(CreateFile(PChar(logPath), GENERIC_WRITE, 0, nil,
        CREATE_NEW, FILE_ATTRIBUTE_NORMAL, 0));
      if handle <> INVALID_HANDLE_VALUE then
        CloseHandle(handle);
      FFile := TLogFileStream.Create(logPath, fmOpenWrite or fmShareDenyWrite);
      break;
    except
      on EFOpenError do
      begin
        if i >= MAX_LOGFILES_PER_PROCESS then
          raise;
        logPath := AppendBeforeExtension(ALogPath, IntToStr(i));
        Inc(i);
      end;
    end;
  FLogLevel := ALogLevel;
  WriteHeaderText;
end;

destructor TFileLogger.Destroy;
begin
  FFile.Free;
  inherited;
end;

procedure TFileLogger.Flush;
begin
  // deprecated
end;

function TFileLogger.GetPath: string;
begin
  Result := FFile.FileName;
end;

function TFileLogger.QueryInterface(const IID: TGUID; out Obj): HResult;
begin
  if GetInterface(IID, Obj) then
    Result := 0
  else
    Result := E_NOINTERFACE;
end;

procedure TFileLogger.WriteHeaderText;
begin
  WriteString(Format('==================== Initializing logger for PID (%d) with LOG_LEVEL=(%s) ===================='#13#10,
    [GetCurrentProcessId, FileLogLevelToStr(LogLevel)]));
end;

procedure TFileLogger.WriteLogEntry(ALogLevel: TFileLogLevel; const AMsg: string);
var
  s: string;
begin
  if ALogLevel > LogLevel then
    Exit;
  s := Format('%s: Status:%s -- Details: Thread Id %d. %s' + #13#10#13#10,
    [FormatDateTime(DATE_FORMAT, Now), FileLogLevelToStr(ALogLevel),
    GetCurrentThreadId, AMsg]);

  WriteString(s);
end;

procedure TFileLogger.WriteLogEntry(ALogLevel: TFileLogLevel; const AMsg: string; E : Exception);
begin
  WriteLogEntry(ALogLevel, Format('Exception: %s. %s', [E.Message, AMsg]));
end;

procedure TFileLogger.WriteString(const s: string);
begin
  FFile.Write(Pointer(s)^, Length(s) * SizeOf(Char));
end;

function TFileLogger._AddRef: Integer;
begin
  Result := -1;
end;

function TFileLogger._Release: Integer;
begin
  Result := -1;
end;

{$IFNDEF LIGHT_ASYNC_LOGGER}

{ TFileLogRequestStrategy }

constructor TFileLogRequestStrategy.Create(const ALogger: TFileLogger);
begin
  FLogger := ALogger;
end;

procedure TFileLogRequestStrategy.Process(const AMsg: IMessage);
begin
  FLogger.WriteLogEntry(Info, Format('Got Request:' + #13#10 +
                               '%s', [AMsg.Bson.asJson]));
end;

{ TFileLogResponseStrategy }

constructor TFileLogResponseStrategy.Create(const ALogger: TFileLogger);
begin
  FLogger := ALogger;
end;

procedure TFileLogResponseStrategy.ProcessResponse(response: IMessage);
begin
  FLogger.WriteLogEntry(Info, Format('Sent Response: ' + #13#10 +
                               '%s', [response.Bson.asJson]));
end;

{$ENDIF}

function FileLogLevelToStr(ALogLevel: TFileLogLevel): string;
begin
  case ALogLevel of
    Always: Result := 'Always';
    Fatal: Result := 'Fatal';
    {$IFNDEF LIGHT_ASYNC_LOGGER}SvcBus.{$ENDIF}Error: Result := 'Error';
    Warning: Result := 'Warning';
    Info: Result := 'Info';
    Debug: Result := 'Debug';
  else
    raise EConvertError.Create('Undefined Log Level');
  end;
end;

function StrToFileLogLevel(const ALogLevel: string): TFileLogLevel;
var
  l: string;
begin
  l := LowerCase(Trim(ALogLevel));
  if l = 'always' then
    Result := Always
  else if l = 'fatal' then
    Result := Fatal
  else if l = 'error' then
    Result := {$IFNDEF LIGHT_ASYNC_LOGGER}SvcBus.{$ENDIF}Error
  else if l = 'warning' then
    Result := Warning
  else if l = 'info' then
    Result := Info
  else if l = 'debug' then
    Result := Debug
  else
    raise EConvertError.Create('Undefined Log Level');
end;

end.

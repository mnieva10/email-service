unit uLogFileStream;

interface

uses Classes;

type  
  TLogFileStream = class(THandleStream)
  strict private
    FFileName: string;
  public
    constructor Create(const AFileName: string; Mode: Word); overload;
    constructor Create(const AFileName: string; Mode: Word; Rights: Cardinal); overload;
    destructor Destroy; override;
    function Write(const Buffer; Count: Longint): Longint; override;
    property FileName: string read FFileName;
  end;

implementation

uses
  SysUtils, Windows, RtlConsts, SyncObjs;

type
  TOverlappedBuffer = packed record
    Overlapped: TOverlapped;
    Data: Byte;
  end;
  POverlappedBuffer = ^TOverlappedBuffer;

const
  PORT_CLOSED = Cardinal(-1);

var
  OpenFiles: Integer;
  CompletionPort: THandle;
  CompletionThreadHandle: THandle;
  CS: TCriticalSection;

function CompletionThreadProc(lpParam: Pointer): Cardinal; stdcall;
const
  ERROR_ABANDONED_WAIT_0 = $02DF;
var
  bResult: BOOL;
  dwNumberOfBytes: Cardinal;
  lpOverlapped: POverlapped;
  LastError: Integer;
  dwCompletionKey: {$IFDEF VER180}DWORD{$ELSE}ULONG_PTR{$ENDIF};
begin
  while True do
  begin
    bResult := GetQueuedCompletionStatus(CompletionPort, dwNumberOfBytes, dwCompletionKey, lpOverlapped, INFINITE);
    if bResult then
    begin
      if lpOverlapped <> nil then
        FreeMem(lpOverlapped);
      if dwNumberOfBytes = PORT_CLOSED then
        Break;
    end
    else
    begin
      LastError := GetLastError;
      if LastError = ERROR_ABANDONED_WAIT_0 then
        Break;
    end;
  end;
  Result := 0;
end;

procedure RegisterFileHandle(FileHandle: THandle);
var
  dwThreadId: Cardinal;
begin
  CS.Enter;
  try
    CompletionPort := CreateIoCompletionPort(FileHandle, CompletionPort, FileHandle, 0);
    if CompletionPort = 0 then
      RaiseLastOSError;
    if CompletionThreadHandle = 0 then
    begin
      CompletionThreadHandle := CreateThread(nil, 0, @CompletionThreadProc, nil, 0, dwThreadId);
      if CompletionThreadHandle = 0 then
      begin
        CloseHandle(CompletionPort);
        CompletionPort := 0;
        RaiseLastOSError;
      end;
    end;
    Inc(OpenFiles);
  finally
    CS.Leave
  end;
end;

procedure UnregisterFileHandle;
begin
  CS.Enter;
  try
    if OpenFiles > 0 then
    begin
      Dec(OpenFiles);
      if OpenFiles = 0 then
      begin
        PostQueuedCompletionStatus(CompletionPort, PORT_CLOSED, 0, nil);
        WaitForSingleObject(CompletionThreadHandle, INFINITE);
        CloseHandle(CompletionThreadHandle);
        CompletionThreadHandle := 0;
        CloseHandle(CompletionPort);
        CompletionPort := 0;
      end;
    end;
  finally
    CS.Leave
  end;
end;

{ TLogFileStream }

constructor TLogFileStream.Create(const AFileName: string; Mode: Word);
begin
  Create(AFilename, Mode, 0);
end;

constructor TLogFileStream.Create(const AFileName: string; Mode: Word;
  Rights: Cardinal);
var
  dwCreationDisposition: Cardinal;
begin
  if Mode = fmCreate then
    dwCreationDisposition := CREATE_ALWAYS
  else
    dwCreationDisposition := OPEN_ALWAYS;
  FHandle := CreateFile(PChar(AFileName), GENERIC_WRITE, FILE_SHARE_READ, nil,
                        dwCreationDisposition, FILE_ATTRIBUTE_NORMAL or FILE_FLAG_OVERLAPPED, 0);
  if FHandle = Integer(INVALID_HANDLE_VALUE) then
  begin
    if Mode = fmCreate then
      raise EFCreateError.CreateResFmt(@SFCreateErrorEx, [ExpandFileName(AFileName), SysErrorMessage(GetLastError)])
    else
      raise EFOpenError.CreateResFmt(@SFOpenErrorEx, [ExpandFileName(AFileName), SysErrorMessage(GetLastError)]);
  end;
  try
    RegisterFileHandle(FHandle);
  except
    FileClose(FHandle);
    raise;
  end;
  inherited Create(FHandle);
  FFileName := AFileName;
end;

destructor TLogFileStream.Destroy;
begin
  if FHandle <> Integer(INVALID_HANDLE_VALUE) then
  begin
    FlushFileBuffers(FHandle);
    FileClose(FHandle);
  end;
  UnregisterFileHandle;
  inherited;
end;

function TLogFileStream.Write(const Buffer; Count: Integer): Longint;
var
  lpOverlappedBuffer: POverlappedBuffer;
  dwBytesWritten, dwError: Cardinal;
  bResult: BOOL;
begin
  GetMem(lpOverlappedBuffer, SizeOf(TOverlapped) + Count);
  with lpOverlappedBuffer^ do
  begin
    Overlapped.Internal := 0;
    Overlapped.InternalHigh := 0;
    Overlapped.Offset := $FFFFFFFF;
    Overlapped.OffsetHigh := $FFFFFFFF;
    Overlapped.hEvent := 0;
    Move(Buffer, Data, Count);
  end;

  bResult := WriteFile(FHandle, Pointer(@lpOverlappedBuffer.Data)^, Count,
                       dwBytesWritten, @lpOverlappedBuffer.Overlapped);
  dwError := GetLastError;

  if not bResult then
  begin
    if dwError <> ERROR_IO_PENDING then
    begin
      FreeMem(lpOverlappedBuffer);
      RaiseLastOSError(dwError);
    end;
  end;

  Result := Count;
end;

initialization
  CS := TCriticalSection.Create;
finalization
  CS.Free;                      
end.

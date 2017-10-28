unit uLogFileStreamTest;

interface

uses TestFramework, uLogFileStream, uJobQueue;

const
  LOG_FILE_COUNT = 3;

type
  TLogArray = array[0..LOG_FILE_COUNT-1] of TLogFileStream;
  {$M+}
  TLogFileStreamTest = class(TTestCase)
  private
    FLog: TLogArray;
    FFileName: array[0..LOG_FILE_COUNT] of string;
    FJobQueue: TJobQueue;
  public
    procedure SetUp; override;
    procedure TearDown; override;
  published
    procedure Test;
  end;
  {$M-}

implementation

uses SysUtils, Classes, SyncObjs, Windows;

const
  STestFileName = 'TLogFileStreamTest%d.log';
  LOOP_COUNT = 1000;
  THREAD_COUNT = 64;
  FIBER_COUNT = 1;

var
  DoneEvent: TEvent;
  DataCounter, JobCounter: Integer;

type
  TTestJob = class(TJob)
  private
    FLog: TLogArray;
  public
    constructor Create(ALog: TLogArray);
    procedure Run; override;
  end;

{ TTestJob }

constructor TTestJob.Create(ALog: TLogArray);
begin
  inherited Create;
  FLog := ALog;
end;

procedure TTestJob.Run;
var
  I, J: Integer;
  S: string;
  threadId: Integer;
begin
  try
    threadId := GetCurrentThreadId;
    for I := 1 to LOOP_COUNT do
    begin
      S := Format('%10d thread: %10d iteration: %10d %' + IntToStr(I) + 's'#13#10,
        [InterlockedIncrement(DataCounter), threadId, I, '']);
      for J := 0 to Length(FLog) - 1 do
        FLog[J].Write(PChar(S)^, {$IFDEF VER180}Length(S){$ELSE}ByteLength(S){$ENDIF});
    end;
  finally
    if InterlockedIncrement(JobCounter) = THREAD_COUNT then
      DoneEvent.SetEvent;
  end;
end;


{ TLogFileStreamTest }

procedure TLogFileStreamTest.SetUp;
var
  I: Integer;
begin
  inherited;
  for I := 0 to LOG_FILE_COUNT - 1 do
  begin
    FFileName[I] := ExtractFilePath(ParamStr(0)) + Format(STestFileName, [I+1]);
    FLog[I] := TLogFileStream.Create(FFileName[I], fmCreate or fmShareDenyWrite);
  end;
  JobCounter := 0;
  DataCounter := 0;
  FJobQueue := TJobQueue.Create([], THREAD_COUNT, FIBER_COUNT);
  DoneEvent.ResetEvent;
end;

procedure TLogFileStreamTest.TearDown;
var
  I: Integer;
begin
  for I := 0 to LOG_FILE_COUNT - 1 do
  begin
    if FLog[I] <> nil then
      FLog[I].Free;
    SysUtils.DeleteFile(FFileName[I]);
  end;
  if Assigned(FJobQueue) then
    FreeAndNil(FJobQueue);
  inherited;
end;

procedure TLogFileStreamTest.Test;
var
  I: Integer;
  S: string;
  Sum, N: Int64;
  {$IFDEF VER180}
  F: Text;
  {$ELSE}
  sr: TStreamReader;
  {$ENDIF}
begin
  for I := 1 to THREAD_COUNT do
  begin
    FJobQueue.WaitUntilWorkerThreadAvailable();
    FJobQueue.Add(TTestJob.Create(FLog));
  end;
  DoneEvent.WaitFor(INFINITE);
  FreeAndNil(FLog);
  Sum := 0;
  {$IFDEF VER180}
  AssignFile(F, FFileName[0]);
  Reset(F);
  {$ELSE}
  sr := TStreamReader.Create(FFileName[0], TEncoding.Unicode);
  {$ENDIF}
  try
    while not {$IFDEF VER180}Eof(F){$ELSE}sr.EndOfStream{$ENDIF} do
    begin
      {$IFDEF VER180}
      Readln(F, S);
      {$ELSE}
      s := sr.ReadLine;
      {$ENDIF}
      Sum := Sum + StrToInt(Trim(Copy(S, 1, 10)));
    end;
  finally
    {$IFDEF VER180}
    CloseFile(F);
    {$ELSE}
    sr.Free;
    {$ENDIF}
  end;
  N := LOOP_COUNT * THREAD_COUNT;
  CheckEquals(((N + 1) * N) div 2, Sum, 'sum');
end;

initialization
  DoneEvent := TEvent.Create(nil, True, False, '');
  RegisterTest(TLogFileStreamTest.Suite);

finalization
  DoneEvent.Free;

end.

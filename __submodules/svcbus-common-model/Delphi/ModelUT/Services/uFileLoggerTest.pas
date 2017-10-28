unit uFileLoggerTest;

interface

uses TestFramework, uFileLogger, uJobQueue;

type
  {$M+}
  TFileLoggerTest = class(TTestCase)
  private
    FLog: TFileLogger;
    FFileName: string;
    FJobQueue: TJobQueue;
  public
    procedure SetUp; override;
    procedure TearDown; override;
  published
    procedure Test;
  end;
  {$M-}

implementation

uses SysUtils, Classes, SyncObjs, Windows, SvcBus;

const
  STestFileName = 'TFileLoggerTest.log';
  LOOP_COUNT = 1000;
  THREAD_COUNT = 64;
  FIBER_COUNT = 1;

var
  DoneEvent: TEvent;
  DataCounter, JobCounter: Integer;

type
  TTestJob = class(TJob)
  private
    FLog: TFileLogger;
  public
    constructor Create(ALog: TFileLogger);
    procedure Run; override;
  end;

{ TTestJob }

constructor TTestJob.Create(ALog: TFileLogger);
begin
  inherited Create;
  FLog := ALog;
end;

procedure TTestJob.Run;
var
  I: Integer;
  S: string;
begin
  try
    for I := 1 to LOOP_COUNT do
    begin
      S := Format('%10d', [InterlockedIncrement(DataCounter)]);
      FLog.WriteLogEntry(Always, S);
    end;
  finally
    if InterlockedIncrement(JobCounter) = THREAD_COUNT then
      DoneEvent.SetEvent;
  end;
end;

{ TFileLoggerTest }

procedure TFileLoggerTest.SetUp;
begin
  inherited;
  FFileName := ExtractFilePath(ParamStr(0)) + STestFileName;
  SysUtils.DeleteFile(FFileName);
  FLog := TFileLogger.Create(FFileName, Always);
  JobCounter := 0;
  DataCounter := 0;
  FJobQueue := TJobQueue.Create([], THREAD_COUNT, FIBER_COUNT);
  DoneEvent.ResetEvent;
end;

procedure TFileLoggerTest.TearDown;
begin
  if FLog <> nil then
    FLog.Free;
  SysUtils.DeleteFile(FFileName);
  if Assigned(FJobQueue) then
    FreeAndNil(FJobQueue);
  inherited;
end;

procedure TFileLoggerTest.Test;
var
  I: Integer;
  F: Text;
  S: string;
  Sum, N: Int64;
begin
  for I := 1 to THREAD_COUNT do
  begin
    FJobQueue.WaitUntilWorkerThreadAvailable();
    FJobQueue.Add(TTestJob.Create(FLog));
  end;
  DoneEvent.WaitFor(INFINITE);
  FreeAndNil(FLog);
  Sum := 0;
  AssignFile(F, FFileName);
  Reset(F);
  try
    while not Eof(F) do
    begin
      Readln(F, S);
      if (Length(S) > 10) and TryStrToInt(Trim(Copy(S, Length(S) - 10, MaxInt)), I) then
        Sum := Sum + I;
    end;
  finally
    CloseFile(F);
  end;
  N := LOOP_COUNT * THREAD_COUNT;
  CheckEquals(((N + 1) * N) div 2, Sum, 'sum');
end;

initialization
  DoneEvent := TEvent.Create(nil, True, False, '');
  RegisterTest(TFileLoggerTest.Suite);

finalization
  DoneEvent.Free;

end.

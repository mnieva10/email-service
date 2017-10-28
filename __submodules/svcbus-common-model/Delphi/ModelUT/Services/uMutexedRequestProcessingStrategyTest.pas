unit uMutexedRequestProcessingStrategyTest;

interface

uses
  TestFramework, uTestUsingDispatcher, SvcBusDispatchInterfaceConsumer,
  SvcBusPipeService, SvcBusService, SvcBusDispatcher,
  uMutexedRequestProcessingStrategy, SvcBusMessage, SvcBusMutex;

type
  {$M+}
  TTestMutexedRequestProcessingStrategy = class(TTestUsingDispatcher, IMutexInfoProvider)
  private
    FDispatcherSvc: IService;
    FDispatcher: TDispatcher;
    FDic: TDispatchInterfaceConsumer;
    FMutex : IMutex;
    function GetMutexName(const AChainMsg : IMessage): String;
    function GetServiceName(const AChainMsg : IMessage): String;
  public
    procedure SetUp; override;
    procedure TearDown; override;
  published
    procedure TestMutexedCommand;
    procedure TestNonMutexedCommand;
  end;
  {$M-}

implementation

uses
  SyncObjs, SvcBusDispatcherResponse, Windows,
  SvcBus, SvcBusConsumer, MongoBsonSerializer, Variants;

type
  {$M+}
  TMutexedRequestProcessingTestDispatchInterface = class(TDispatchInterface)
  published
    function SomeJob(msg : IMessage): TObject;
  end;
  {$M-}

var
  TimeProcessed : Cardinal;
  ProcessedCount : Integer;

function TTestMutexedRequestProcessingStrategy.GetMutexName(const AChainMsg : IMessage): String;
var
  Obj : TStatusResponse;
begin
  Obj := nil;
  Result := '';
  if AChainMsg <> nil then
  begin
    TDispatchInterface.DeserializeRequest(AChainMsg, TObject(Obj), TStatusResponse, nil);
    if not VarIsEmpty(Obj.Status) then
      Result := Obj.Status;
  end;
end;

function TTestMutexedRequestProcessingStrategy.GetServiceName(const AChainMsg : IMessage): String;
begin
  Result := 'TEST_MODEL_SERVICE';
end;

{ TTestMutexedRequestProcessingStrategy }

procedure TTestMutexedRequestProcessingStrategy.TestMutexedCommand;
var
  StartTickCount : Cardinal;
  Obj : TStatusResponse;
begin
  FMutex.Acquire;
  FDic.Consumer.setResponseTimeout(10000);
  StartTickCount := GetTickCount;
  Obj := TStatusResponse.Create;
  try
    Obj.Status := 'Hello';
    FDic.Send('SomeJob', Obj);
    FDic.Send('SomeJob', Obj);
  finally
    Obj.Free;
  end;
  Sleep(2000);
  FMutex.Release;
  while TimeProcessed = 0 do
    Sleep(1);
  Check(abs(TimeProcessed - StartTickCount) > 2000, 'Time processing was too fast');
  while ProcessedCount < 2 do
    Sleep(1);
  CheckEquals(2, ProcessedCount);
end;

procedure TTestMutexedRequestProcessingStrategy.SetUp;
begin
  inherited;
  TimeProcessed := 0;
  ProcessedCount := 0;
  FMutex := newMutex(Fps, 'Hello', GetServiceName(nil));
  FMutex.Remove;
  FMutex := newMutex(Fps, 'Hello', GetServiceName(nil));
  FDispatcherSvc := newService('TTestMutexedRequestProcessingStrategy', VOLATILE);

  FDispatcher := TDispatcher.Create('TTestMutexedRequestProcessingStrategy',
    FDispatcherSvc, FPs, TMutexedRequestProcessingTestDispatchInterface);
  FDispatcher.RequestProcessingStrategy := TMutexedRequestProcessingStrategy.Create(FPs, Self, nil);
  FDispatcher.Start(2, dmSingletonDispatchInterface);

  FDic := TDispatchInterfaceConsumer.Create(FPs, 'ServiceControllerPingListenerTest',
                FDispatcherSvc,
                SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
end;

procedure TTestMutexedRequestProcessingStrategy.TearDown;
begin
  FMutex := nil;
  inherited;
end;

{ TTestMutexedRequestProcessingStrategy }

procedure TTestMutexedRequestProcessingStrategy.TestNonMutexedCommand;
var
  StartTickCount : Cardinal;
  Obj : TStatusResponse;
begin
  FMutex.Acquire;
  FDic.Consumer.setResponseTimeout(10000);
  StartTickCount := GetTickCount;
  Obj := TStatusResponse.Create;
  try
    FDic.Send('SomeJob', Obj);
  finally
    Obj.Free;
  end;
  Sleep(20);
  while TimeProcessed = 0 do
    Sleep(1);
  Check(abs(TimeProcessed - StartTickCount) < 100, 'Time processing was too slow');
end;

{ TMutexedRequestProcessingTestDispatchInterface }

function TMutexedRequestProcessingTestDispatchInterface.SomeJob(msg : IMessage): TObject;
begin
  TimeProcessed := GetTickCount;
  InterlockedIncrement(ProcessedCount);
  Result := nil;
end;

initialization
  RegisterTest(TTestMutexedRequestProcessingStrategy.Suite);
end.

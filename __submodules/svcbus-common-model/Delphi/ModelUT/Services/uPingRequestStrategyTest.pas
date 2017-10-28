unit uPingRequestStrategyTest;

interface

uses
  TestFramework, uTestUsingDispatcher, SvcBusDispatchInterfaceConsumer,
  SvcBusPipeService, SvcBusService, SvcBusDispatcher;

type
  {$M+}
  TTestDestinationDispatcherConfigurable = class(TTestUsingDispatcher)
  private
    FDispatcherSvc: IService;
    FDispatcher: TDispatcher;
    FDic: TDispatchInterfaceConsumer;
  public
    procedure SetUp; override;
    procedure TearDown; override;
  published
    procedure PingRequestReceivedWhenAllWorkersBusy_ShouldRespondFromMainThread;
  end;
  {$M-}

implementation

uses
  SvcBusMessage, SyncObjs, SvcBusDispatcherResponse, Windows, uPingRequestStrategy,
  SvcBus, SvcBusConsumer;

var
  FinishLongJobEvent: TEvent;

type
  {$M+}
  TPingStrategyTestDispatchInterface = class(TDispatchInterface)
  published
    function LongJob(msg : IMessage): TObject;
  end;
  {$M-}

{ TTestDestinationDispatcherConfigurable }

procedure TTestDestinationDispatcherConfigurable.PingRequestReceivedWhenAllWorkersBusy_ShouldRespondFromMainThread;
begin
  FDic.Consumer.setResponseTimeout(500);
  FDic.Send('LongJob', TStatusResponse.Create);
  StartExpectingException(EConsumer);
  FDic.sendAndWait('LongJob', TStatusResponse.Create);
  StopExpectingException();
  FDic.sendAndWait('Ping', TStatusResponse.Create);
end;

procedure TTestDestinationDispatcherConfigurable.SetUp;
begin
  inherited;
  FinishLongJobEvent.ResetEvent;

  FDispatcherSvc := newService('TTestDestinationDispatcherConfigurable', VOLATILE);

  FDispatcher := TDispatcher.Create('TTestDestinationDispatcherConfigurable',
    FDispatcherSvc, FPs, TPingStrategyTestDispatchInterface);
  FDispatcher.YieldTimeout := 0;
  FDispatcher.RequestProcessingStrategy := TPingRequestStrategy.Create;
  FDispatcher.Start(1, dmSingletonDispatchInterface);

  FDic := TDispatchInterfaceConsumer.Create(FPs, 'ServiceControllerPingListenerTest',
                FDispatcherSvc,
                SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
end;

procedure TTestDestinationDispatcherConfigurable.TearDown;
begin
  FinishLongJobEvent.SetEvent;
  inherited;
end;

{ TPingStrategyTestDispatchInterface }

function TPingStrategyTestDispatchInterface.LongJob(msg: IMessage): TObject;
begin
  FinishLongJobEvent.WaitFor(INFINITE);
  Result := TStatusResponse.Create;
end;

initialization
  RegisterTest(TTestDestinationDispatcherConfigurable.Suite);
  FinishLongJobEvent := TEvent.Create(false);
finalization
  FinishLongJobEvent.Free;

end.

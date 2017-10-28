unit uDestinationDispatcherConfigurableTest;

interface

uses
  TestFramework, SvcBusDispatchInterfaceConsumer,
  SvcBusPipeService, SvcBusService, SvcBusDispatcher, uTestUsingDispatcher;

type
  {$M+}
  TTestDestinationDispatcherConfigurable = class(TTestUsingDispatcher)
  private
    FDispatcherSvc: IService;
    FDispatcher: TDispatcher;
    FDic: TDispatchInterfaceConsumer;
    function Ping(AMachineName: string = ''; APid: Integer = -1): Boolean;
  public
    procedure SetUp; override;
    procedure TearDown; override;
  published
    procedure PingRequestWithDifferentPid_ShouldSkip;
    procedure PingRequestWithDifferentMachineName_ShouldSkip;
    procedure PingRequestWithMatchingMachineNameAndPid_ShouldSendResponse;
  end;
  {$M-}
implementation

uses
  SvcBus, uDestinationDispatcherConfigurable,
  uDestinationRequestProcessingStrategy, Windows;

{ TTestDestinationDispatcherConfigurable }

procedure TTestDestinationDispatcherConfigurable.SetUp;
begin
  inherited;
  FDispatcherSvc := newService('ServiceControllerPingListenerTest', VOLATILE);

  FDispatcher := TDispatcher.Create('AuthorizationDispatcher', FDispatcherSvc, FPs,
                TDispatchInterface);
  FDispatcher.RequestProcessingStrategy := TDestinationRequestProcessingStrategy.Create;
  FDispatcher.Start(1, dmSingletonDispatchInterface);

  FDic := TDispatchInterfaceConsumer.Create(FPs, 'ServiceControllerPingListenerTest',
                FDispatcherSvc,
                SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
end;

procedure TTestDestinationDispatcherConfigurable.TearDown;
begin
  FDispatcher.Free;
  FDic.Free;
  FDispatcherSvc := nil;
  inherited;
end;

function TTestDestinationDispatcherConfigurable.Ping(AMachineName: string; APid: Integer): Boolean;
var
  req: TDestinationDispatcherConfigurable;
begin
  req := TDestinationDispatcherConfigurable.Create;
  if AMachineName <> '' then
    req.MachineName := AMachineName;
  if APid <> -1 then
    req.Pid := APid;
  try
    FDic.SendAndWait('Ping', req);
    Result := true;
  except
    Result := false;
  end;
end;

procedure TTestDestinationDispatcherConfigurable.PingRequestWithDifferentMachineName_ShouldSkip;
begin
  CheckFalse(Ping('Invalid machine name'));
end;

procedure TTestDestinationDispatcherConfigurable.PingRequestWithDifferentPid_ShouldSkip;
begin
  CheckFalse(Ping(GetComputerNameAsString, GetCurrentProcessId + 1));
end;

procedure TTestDestinationDispatcherConfigurable.PingRequestWithMatchingMachineNameAndPid_ShouldSendResponse;
begin
  CheckTrue(Ping);
end;

initialization
  RegisterTest(TTestDestinationDispatcherConfigurable.Suite);

end.

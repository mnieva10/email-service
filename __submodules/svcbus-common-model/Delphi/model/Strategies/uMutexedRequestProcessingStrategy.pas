unit uMutexedRequestProcessingStrategy;

interface

uses
  SvcBusDispatcher, SvcBusMessage, SvcBusPipeService, StringHashTrie,
  uLinkedList, SyncObjs, SvcBusMutex;

type
  IMutexInfoProvider = interface
    ['{2325A920-E77E-4DA3-8FCB-1FDD6ED42CD0}']
    function GetMutexName(const AChainMsg : IMessage) : String;
    function GetServiceName(const AChainMsg : IMessage) : String;
  end;

  TMutexedRequestProcessingStrategy = class(TChainableRequestProcessingStrategy)
  private
    FPs : IPipeService;
    FMutexInfoProvider : IMutexInfoProvider;
    FMsgToMutexDictionary : TStringHashTrie;
    FOnHoldMessages : ILinkedList;
    FDictionaryLock : TCriticalSection;
    function AcquireMutex(const AChainMsg: IMessage; const AMutexName, ASvcName: String): Boolean;
    function GetMutex(const AMsg: IMessage): IMutex;
  public
    constructor Create(Aps: IPipeService; AMutexInfoProvider: IMutexInfoProvider; ANext: TChainableRequestProcessingStrategy);
    destructor Destroy; override;
    function GetMessage: IMessage; overload; override;
    procedure RequestComplete(const AMsg: IMessage); override;
    function TakeMessage(const AMsg : IMessage): Boolean; override;
  end;

implementation

uses
  SvcBus, MongoBson, SysUtils, iContainers, Hash_Trie;

{ TMutexedRequestProcessingStrategy }

constructor TMutexedRequestProcessingStrategy.Create(Aps: IPipeService; AMutexInfoProvider: IMutexInfoProvider; ANext: TChainableRequestProcessingStrategy);
begin
  inherited Create(ANext);
  Assert(Assigned(Aps));
  Assert(Assigned(AMutexInfoProvider));
  Fps := Aps;
  FMutexInfoProvider := AMutexInfoProvider;
  FMsgToMutexDictionary := TStringHashTrie.Create;
  FMsgToMutexDictionary.AutoFreeValue := True;
  FMsgToMutexDictionary.AutoFreeValueMode := afmReleaseInterface;
  FOnHoldMessages := LinkedListFactory.CreateObject(Self) as ILinkedList;
  FDictionaryLock := TCriticalSection.Create;
end;

destructor TMutexedRequestProcessingStrategy.Destroy;
begin
  if FMsgToMutexDictionary <> nil then
    FreeAndNil(FMsgToMutexDictionary);
  if FDictionaryLock <> nil then
    FreeAndNil(FDictionaryLock);
  FOnHoldMessages := nil;
  inherited;
end;

function TMutexedRequestProcessingStrategy.AcquireMutex(const AChainMsg: IMessage; const AMutexName, ASvcName: String): Boolean;
var
  AMutex : IMutex;
begin
  AMutex := newMutex(Fps, AMutexName, ASvcName);
  FDictionaryLock.Enter;
  try
    FMsgToMutexDictionary.Add(AnsiString(AChainMsg.Id.asString), AMutex as IUnknown);
  finally
    FDictionaryLock.Leave;
  end;
  Result := AMutex.Acquire;
end;

function TMutexedRequestProcessingStrategy.GetMessage: IMessage;
var
  AMutex : IMutex;
begin
  with NewIterator(FOnHoldMessages) do
    while IterateForward do
    begin
      Result := GetAsInterface as IMessage;
      AMutex := GetMutex(Result);
      Assert(AMutex <> nil);
      if AMutex.Acquire then
      begin
        FOnHoldMessages.RemoveNode(Context);
        exit;
      end;
    end;
  Result := inherited GetMessage;
end;

function TMutexedRequestProcessingStrategy.GetMutex(const AMsg: IMessage): IMutex;
var
  AMutexUnknown : IUnknown;
begin
  FDictionaryLock.Enter;
  try
    FMsgToMutexDictionary.Find(AnsiString(AMsg.Id.asString), AMutexUnknown);
  finally
    FDictionaryLock.Leave;
  end;
  Result := AMutexUnknown as IMutex;
end;

procedure TMutexedRequestProcessingStrategy.RequestComplete(const AMsg: IMessage);
var
  AMutex : IMutex;
begin
  AMutex := GetMutex(AMsg);
  if AMutex <> nil then
  begin
    FDictionaryLock.Enter;
    try
      FMsgToMutexDictionary.Remove(AnsiString(AMsg.Id.asString));
    finally
      FDictionaryLock.Leave;
    end;
    AMutex.Release;
  end;
end;

function TMutexedRequestProcessingStrategy.TakeMessage(const AMsg : IMessage): Boolean;
var
  AMutexName : String;
  ASvcName : String;
  AMutex : IMutex;
begin
  AMutex := GetMutex(AMsg);
  if AMutex <> nil then
  begin
    Result := True;
    exit; // Mutex was pre-acquired on prior call to GetMessage()
  end;
  Result := inherited TakeMessage(AMsg);
  if not Result then
    exit;
  AMutexName := FMutexInfoProvider.GetMutexName(AMsg);
  ASvcName := FMutexInfoProvider.GetServiceName(AMsg);
  if (AMutexName <> '') and (ASvcName <> '') and (not AcquireMutex(AMsg, AMutexName, ASvcName)) then
  begin
    FOnHoldMessages.Insert(AMsg);
    Result := False;
  end;
end;

end.

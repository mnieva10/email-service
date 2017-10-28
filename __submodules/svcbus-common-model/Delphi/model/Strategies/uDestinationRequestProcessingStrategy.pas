unit uDestinationRequestProcessingStrategy;

interface

uses
  SvcBusDispatcher, SvcBusMessage, uDestinationDispatcherConfigurable;

type
  TDestinationRequestProcessingStrategy = class(TChainableRequestProcessingStrategy)
  private
    function ExtractDestination(const AMsg: IMessage): TDestinationDispatcherConfigurable;
  public
    function GetMessage(const AChainMsg: IMessage): IMessage; override;
  end;

implementation

uses
  MongoBson, SvcBus, MongoBsonSerializer;

{ TDestinationRequestProcessingStrategy }

function TDestinationRequestProcessingStrategy.GetMessage(
  const AChainMsg: IMessage): IMessage;
var
  dest: TDestinationDispatcherConfigurable;
begin
  dest := ExtractDestination(AChainMsg);
  try
    if (dest <> nil) and (not dest.MatchCurrent) then
    begin
      Result := nil;
      Exit;
    end;
    if Next <> nil then
      Result := Next.GetMessage(AChainMsg)
    else
      Result :=  AChainMsg;
  finally
    dest.Free;
  end;
end;

function TDestinationRequestProcessingStrategy.ExtractDestination(
  const AMsg: IMessage): TDestinationDispatcherConfigurable;
var
  it: IBsonIterator;
  deserializer: TBaseBsonDeserializer;
begin
  it := AMsg.bson.find(SVCBUS_DISPATCHER_DESCRIBE_PARAMETERS);
  if it = nil then
  begin
    Result := nil;
    Exit;
  end;

  deserializer := CreateDeserializer(TObject);
  Result := TDestinationDispatcherConfigurable.Create;
  try
    deserializer.Source := it.subiterator;
    deserializer.Deserialize(TObject(Result), nil);
  finally
    deserializer.Free;
  end;
end;

end.

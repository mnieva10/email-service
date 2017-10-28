unit uPingRequestStrategy;

interface

uses
  SvcBusDispatcher, SvcBusMessage;

type
  TPingRequestStrategy = class(TChainableRequestProcessingStrategy)
  public
    function GetMessage(const AChainMsg: IMessage): IMessage; override;
  end;

implementation

uses
  SvcBus, SysUtils, MongoBson;

{ TPingRequestStrategy }

function TPingRequestStrategy.GetMessage(const AChainMsg: IMessage): IMessage;
var
  emptyResponse: IMessage;
  it: IBsonIterator;
begin
  try
    it := AChainMsg.Bson.find(SVCBUS_DISPATCHER_COMMAND);
    if (it <> nil) and (LowerCase(it.AsUTF8String) = 'ping') then
    begin
      if Producer.take(AChainMsg) then
      begin
        emptyResponse := newMessage(AChainMsg.messageId);
        Producer.responder.send(AChainMsg.responsePipeName, emptyResponse);
      end;
      Result := nil;
    end;
  except
  end;
  if Next <> nil then
    Result := Next.GetMessage(AChainMsg)
  else
    Result := AChainMsg;
end;

end.

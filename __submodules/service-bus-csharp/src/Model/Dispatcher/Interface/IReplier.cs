using System;

namespace Sovos.SvcBus
{
  public interface IReplier
  {
    void send(string responsePipeName, Message msg);
    void send(string responsePipeName, object message,
              Oid msgId, IResponseProcessingStrategy responseProcessingStrategy, 
              bool isBroadcast);

    void ResponseFromException(Exception e, Bson bson);
    Message ResponseFromException(Oid msgId, Exception e);
    void RegisterAsyncReply(Oid messageId, Oid statReportId);
  }
}
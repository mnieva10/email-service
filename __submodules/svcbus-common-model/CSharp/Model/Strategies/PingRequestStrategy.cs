using System;

namespace Sovos.SvcBus.Common.Model.Strategies
{
    /// <summary>
    /// accept ping requests in ProducerJob thread
    /// this allows to accept ping requests even if all DispatchJob threads busy
    /// its a bit hacky but it works
    /// </summary>
    public class PingRequestStrategy : ChainableRequestProcessingStrategy
    {
        private readonly bool _removeMsgPostProcessing;

        public PingRequestStrategy(ChainableRequestProcessingStrategy next = null, bool removeMsgPostProcessing = false)
            : base(next)
        {
            _removeMsgPostProcessing = removeMsgPostProcessing;
        }

        public override Message GetMessage(Message chainMsg)
        {
            try
            {
                if (string.Equals("ping", (string) chainMsg.bson.find(DispatcherConstants.Command),
                    StringComparison.CurrentCultureIgnoreCase) && Producer.take(chainMsg))
                {
                    var result = Builder.newMessage(chainMsg.messageId);
                    var sub = result.bson.appendDocumentBegin(DispatcherConstants.Response);
                    sub.append("_type", "DataDto");
                    sub.append("DataString", "ok");
                    sub.append("DataBool", true);
                    result.bson.appendDocumentEnd(sub);

                    var prod = (Producer)Producer;
                    prod.responder.send(chainMsg.responsePipeName, result);
                    if (_removeMsgPostProcessing)
                        prod.remove(chainMsg._id);
                    return null;
                }
            }
            catch { }
            return Next != null ? Next.GetMessage(chainMsg) : chainMsg;
        }
    }
}

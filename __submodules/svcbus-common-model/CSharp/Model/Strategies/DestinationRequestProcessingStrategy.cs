using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Strategies
{
    public class DestinationRequestProcessingStrategy : ChainableRequestProcessingStrategy
    {
        public DestinationRequestProcessingStrategy(ChainableRequestProcessingStrategy next = null)
            : base(next)
        {
        }

        private DestinationDispatcherConfigurable ExtractDestination(Message msg)
        {
            var it = msg.bson.find(DispatcherConstants.Params);
            if (it == null)
                return null;

            object o = new DestinationDispatcherConfigurable();
            new BsonDeserializer(Builder.newIterator(it)).Deserialize(ref o);
            return (DestinationDispatcherConfigurable) o;
        }

        public override Message GetMessage(Message chainMsg)
        {
            var dest = ExtractDestination(chainMsg);
            if (dest != null && !dest.MatchCurrent)
                return null;
            if (Next != null)
                return Next.GetMessage(chainMsg);
            return chainMsg;
        }
    }
}
namespace Sovos.SvcBus
{
  internal class DefaultResponseProcessingStrategy : IResponseProcessingStrategy
  {
    public void ProcessResponse(Message response)
    {
      /* We don't do nothing to the response by default. Other implementors may decided to inject data here to
       every request before sending to consumer. This is a nice way to run Producers in "debug" mode and inject
       state information to every request that can "peeked" on the bus */
    }
  }
}
namespace Sovos.SvcBus
{
  public interface IResponseProcessingStrategy
  {
    void ProcessResponse(Message response);
  }
}
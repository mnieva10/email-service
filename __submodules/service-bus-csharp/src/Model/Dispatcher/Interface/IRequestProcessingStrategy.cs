namespace Sovos.SvcBus
{
  public interface IRequestProcessingStrategy
  {
    Message GetMessage();
    void Process(Message msg);
    IProducer Producer { get; set; }
    void RequestComplete(Message msg);
    bool TakeMessage(Message msg);
  }
}
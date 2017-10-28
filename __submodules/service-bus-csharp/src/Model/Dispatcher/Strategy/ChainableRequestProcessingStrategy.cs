using System;

namespace Sovos.SvcBus
{
  public class ChainableRequestProcessingStrategy : IRequestProcessingStrategy
  {
    public ChainableRequestProcessingStrategy(ChainableRequestProcessingStrategy next = null)
    {
      if (next != null)
      {
        Next = next;
        Next.Prev = this;
      }
    }

    protected ChainableRequestProcessingStrategy Next { get; set; }
    protected ChainableRequestProcessingStrategy Prev { get; set; }

    public virtual Message GetMessage(Message chainMsg)
    {
      return Next != null ? Next.GetMessage(chainMsg) : chainMsg;
    }

    public virtual Message GetMessage()
    {
      return GetMessage(Producer.wait());
    }

    public virtual void Process(Message msg)
    {
      if (Next != null)
        Next.Process(msg);
      GC.KeepAlive(msg);
    }

    private IProducer _producer;
    public IProducer Producer
    {
      get
      {
        return Prev != null ? Prev.Producer : _producer;
      }

      set
      {
        if (Prev != null)
          Prev.Producer = value;
        else
          _producer = value;
      }
    }

    public virtual void RequestComplete(Message msg)
    {
    }

    public virtual bool TakeMessage(Message msg)
    {
      return Producer.take(msg);
    }
  }
}
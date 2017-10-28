using System;

namespace Sovos.SvcBus
{
  [Serializable]
  public class ThreadCountException : DispatcherException
  {
    public ThreadCountException() : base(DispatcherConstants.WrongThreadCountParameter) { }
  }
}
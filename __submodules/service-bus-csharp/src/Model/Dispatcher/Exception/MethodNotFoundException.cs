using System;

namespace Sovos.SvcBus
{
  [Serializable]
  public class MethodNotFoundException : DispatcherException
  {
    public MethodNotFoundException(string command) : base(string.Format(DispatcherConstants.MethodNotFoundInDispatcherInterface, command)) { }
  }
}
using System;

namespace Sovos.SvcBus
{
  [Serializable]
  public class ServiceNameException : DispatcherException
  {
    public ServiceNameException() : base(DispatcherConstants.DispatcherNameCanTBeBlank) { }
  }
}
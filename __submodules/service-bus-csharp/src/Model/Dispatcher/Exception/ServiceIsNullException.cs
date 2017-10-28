using System;

namespace Sovos.SvcBus
{
  [Serializable]
  public class ServiceIsNullException : DispatcherException
  {
    public ServiceIsNullException() : base(DispatcherConstants.ServiceCanTBeNil) { }
  }
}
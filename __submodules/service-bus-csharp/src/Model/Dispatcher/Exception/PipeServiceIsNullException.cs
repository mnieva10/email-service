using System;

namespace Sovos.SvcBus
{
  [Serializable]
  public class PipeServiceIsNullException : DispatcherException
  {
    public PipeServiceIsNullException() : base(DispatcherConstants.PipeServiceCanTBeNil) { }
  }
}
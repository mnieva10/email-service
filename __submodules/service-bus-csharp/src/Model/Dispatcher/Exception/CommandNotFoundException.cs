using System;

namespace Sovos.SvcBus
{
  [Serializable]
  public class CommandNotFoundException : DispatcherException
  {
    public CommandNotFoundException() : base(DispatcherConstants.CommandStringNotFoundInMessage) { }
  }
}
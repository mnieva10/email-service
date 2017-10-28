using System;

namespace Sovos.SvcBus
{
  [Serializable]
  public class DispatcherException : Exception
  {
    private readonly string _originalExceptionClassName;

    public DispatcherException(string message) : base(message) { }
    public DispatcherException(string message, Exception innerException) : base(message, innerException) { }

    public DispatcherException(string message, string originalExceptionClassName)
      : base(message)
    {
      _originalExceptionClassName = originalExceptionClassName;
    }

    public string OriginalExceptionClassName
    {
      get
      {
        return _originalExceptionClassName;
      }
    }
  }
}
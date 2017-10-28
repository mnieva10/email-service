namespace Sovos.SvcBus
{
  public static class DispatcherConstants
  {
    public const string Response = "response";
    public const string Command = "command";
    public const string Commands = "commands";
    public const string ActualType = "_type";
    public const string CollectionKey = "_key";
    public const string CollectionValue = "_value";

    public const string Describe = "Describe";
    public const string Version = "Version";
    public const string Priority = "DispatcherPriority";

    public const string Description = "description";
    public const string Params = "params";
    public const string Result = "result";
    public const string Throws = "throws";

    public const string Exception = "exception";
    public const string ExceptionMessage = "message";
    public const string ExceptionErrorCode = "errorCode";
    public const string ExceptionSourceMessage = "sourceMessage";
    public const string StackInfo = "stackInfo";

    public const string ReturnsTheInterfaceVersionNumber = "Returns the interface version number";
    public const string DescribeDescription = "'describe' returns the description of every supported method";
    public const string DispatcherNameCanTBeBlank = "Dispatcher Name cannot be blank";
    public const string PipeServiceCanTBeNil = "Pipe Service cannot be null";
    public const string ServiceCanTBeNil = "Service cannot be null";
    public const string WrongThreadCountParameter = "Thread Count must be >= 1";
    public const string MethodNotFoundInDispatcherInterface = "Method '{0}' was not found in the provided Dispatch Interface";
    public const string CommandStringNotFoundInMessage = "'command' string was not found in the message";
    public const string CouldNotFindPropInfoForClass = "Deserializer internal error. Could not find PropInfo for target object class";
  }
}
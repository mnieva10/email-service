/* WARNING:

   Please note that since version 2.7.1 of service bus is mandatory touse Helper.SetLogger()
   to set error reporting interface when using Dispatcher functionality or Consumer
   with Async wait for Response functionality. If this is not done, application will fail
   to operate properly raising an exception */

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using GChelpers;

namespace Sovos.SvcBus
{
  using SvcBusGenericErr = IntPtr;

  /// <summary>
  /// Setting config file to Always only writes log entries where WriteLogEntry was called with Always.
  /// Setting config file to Debug logs EVERYTHING.
  /// WriteLogEntry writes to the log file only when it is called with a log level less than or equal to the config file's log level.
  /// </summary>
  public enum LogLevel
  {
    Always = 0,
    Fatal = 1,
    Error = 2,
    Warning = 3,
    Info = 4,
    Debug = 5,
  }

  public interface ILogger
  {
    void WriteLogEntry(LogLevel logLevel, string message);

    void WriteLogEntry(object sender, Exception e, string msg, LogLevel logLevel);
  }

  public class SimpleConsoleLogger : ILogger
  {
    public void  WriteLogEntry(LogLevel logLevel, string message)
    {
      Console.WriteLine("{0}, {1}", logLevel, message);
    }

    public void WriteLogEntry(Object sender, Exception e, string msg, LogLevel logLevel)
    {
      Console.WriteLine("{0} Exception: {1}. {2}", logLevel, e, msg);
    }
  }

  [Serializable]
  public class SvcBusException : Exception
  {
    private const string SCode = "Code";
    private const string SSourceMessage = "SourceMessage";
    protected SvcBusException(SerializationInfo info, StreamingContext context)
#if !NETCORE
      : base(info, context)
#endif
    {
      ErrorCode = info.GetInt32(SCode);
      SourceMessage = info.GetString(SSourceMessage);
    }
    public SvcBusException(string msg, int code, string sourceMsg)
      : base(string.Format("{0} [{1} : {2}]", msg, code, sourceMsg))
    {
      ErrorCode = code;
      SourceMessage = sourceMsg;
    }

    public int ErrorCode { get; private set; }
    public string SourceMessage { get; private set; }

#if !NETCORE
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException("info");
      base.GetObjectData(info, context);
      info.AddValue(SCode, ErrorCode);
      info.AddValue(SSourceMessage, SourceMessage);
    }
  }
#endif

#if NETCORE
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException("info");
      info.AddValue(SCode, ErrorCode);
      info.AddValue(SSourceMessage, SourceMessage);
     }
   }
#endif

  public class GenericErr
  {
    public SvcBusGenericErr Handle { get; protected set; }

    public int ErrCode
    {
      get
      {
        return NativeMethods.SvcBusGenericErr_t_getErr(Handle);
      }
    }

    public string ErrMsg
    {
      get
      {
        return Marshal.PtrToStringAnsi(NativeMethods.SvcBusGenericErr_t_getErrstr(Handle));
      }
    }

    public GenericErr()
    {
      Handle = NativeMethods.SvcBusGenericErr_t_create();
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle, Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusGenericErr_t_free));
    }

    public GenericErr(SvcBusGenericErr handle)
    {
      Handle = handle;
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle);
    }

    ~GenericErr()
    {
      if (Handle == SvcBusGenericErr.Zero) 
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = SvcBusGenericErr.Zero;
    }
  }

  public class SvcBusHandles : HandleCollection<Type, IntPtr> { }

  public class Utils
  {
    public class SvcBusHandleTracker : UnmanagedObjectGCHelper<Type, IntPtr> { }
    public static SvcBusHandleTracker UnmanagedObjectTracker { get; private set; }

    public static UnmanagedObjectContext<Type, IntPtr>.DestroyHandleDelegate SvcBusHandleDestroyDelegate(
      UnmanagedObjectContext<Type, IntPtr>.DestroyHandleDelegate function)
    {
      return function;
    }
  
    static Utils()
    {
      UnmanagedObjectTracker = new SvcBusHandleTracker();
      AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
      {
        UnmanagedObjectTracker.Dispose();
      };
    }

    public static bool RunningOnTestFramework()
    {
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        var assemblyNameUpper = assembly.GetName().Name.ToUpper();
        if (assemblyNameUpper.Contains("NUNIT.FRAMEWORK") ||
            assemblyNameUpper.Contains("UNITTESTFRAMEWORK") ||
            assemblyNameUpper.Contains("WEBTESTFRAMEWORK"))
          return true;
      }
      return false;
    }

    public static IntPtr NativeUtf8FromString(string managedString)
    {
      var buffer = Encoding.UTF8.GetBytes(managedString);
      Array.Resize(ref buffer, buffer.Length + 1);
      buffer[buffer.Length - 1] = 0;
      var nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
      Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
      return nativeUtf8;
    }

    public static string StringFromNativeUtf8(IntPtr nativeUtf8, uint len)
    {
      if(len == 0)
        return string.Empty;
      var i = 0;
      var buffer = len == 0 ? new byte[16] : new byte[len + 1];
      byte b;
      while ((b = Marshal.ReadByte(nativeUtf8, i)) != 0)
      {
        if (buffer.Length <= i)
        {
          Array.Resize(ref buffer, buffer.Length * 2);
        }
        buffer[i++] = b;
      }
      return Encoding.UTF8.GetString(buffer, 0, i);
    }

    public static DateTime ToDateTimeFromMillisecondsSinceEpoch(long millisecondsSinceEpoch)
    {
      var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      return origin.AddMilliseconds(millisecondsSinceEpoch).ToLocalTime();
    }

    public static long ToMillisecondsSinceEpoch(DateTime dateTime)
    {
      return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    /* the following method allows comparing a C# native DateTime variable with one coming from MongoDb ignoring the ticks portion of the
       DateTime parameters.
       See article: http://alexmg.com/datetime-precision-with-mongodb-and-the-c-driver/ 
     */
    public static int MilisecondsPrecisionCompare(DateTime t1, DateTime t2)
    {
      long diff = t1.Ticks - t2.Ticks;
      if (diff < TimeSpan.TicksPerMillisecond) // assume equal
        return 0;
      return DateTime.Compare(t1, t2);
    }
  }
}

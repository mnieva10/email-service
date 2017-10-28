using System;
using System.Collections;
using System.Linq;

namespace Sovos.SvcBus
{
  public static class TypeExtensions
  {
    public static bool ImplementsIDictionary(this Type type)
    {
      return type.GetInterfaces().Any(interf => interf == typeof (IDictionary));
    }

    public static bool ImplementsIList(this Type type)
    {
      return type.GetInterfaces().Any(interf => interf == typeof (IList));
    }

    public static bool ImplementsGenericEnumerable(this Type type)
    {
      //public sealed class String : IComparable, ICloneable, IConvertible, IComparable<string>, IEnumerable<char>, IEnumerable, IEquatable<string>
      if (type.GetTypeInfo().IsGenericType && type != typeof (string))
        return type.GetInterfaces().Any(interf => interf == typeof (IEnumerable));
      return false;
    }

#if !NETCORE
    public static Type GetTypeInfo(this Type type)
    {
      return type;
    }
#else
    public static bool IsAssignableFrom(this Type type, Type t)
    {
      return type.GetTypeInfo().IsAssignableFrom(t);
    }

    public static MethodInfo GetMethod(this Type type, string name)
    {
      return type.GetTypeInfo().GetMethod(name);
    }

    public static bool Change(this Timer timer, uint dueTime, uint period)
    {
      return timer.Change((int)dueTime, (int)period);
    }

    public static StackFrame GetFrame(this StackTrace st, int index)
    {
      StackFrame[] frames = st.GetFrames();
      if (frames != null && frames.Length > index)
      {
        return frames[index];
      }
      else
        return null;
    }
#endif
  }
}


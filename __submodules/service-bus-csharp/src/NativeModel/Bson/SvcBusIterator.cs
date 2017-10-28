using System;
using System.Runtime.InteropServices;

namespace Sovos.SvcBus
{
  public class Iterator
  {
    public bson_type bsonType {
      get 
      {
        var result = (bson_type)NativeMethods.bson_iter_type(it);
        GC.KeepAlive(this);
        return result;
      }
    }

    public string key {
      get
      {
        var result = Marshal.PtrToStringAnsi(NativeMethods.bson_iter_key(it));
        GC.KeepAlive(this);
        return result;
      }
    }

    public IntPtr it { get; set; }

    ~Iterator()
    {
      if (it == IntPtr.Zero) 
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), it);
      it = IntPtr.Zero;
    }

    public static explicit operator int(Iterator iterator) 
    {
      var result =  NativeMethods.bson_iter_int32(iterator.it);
      GC.KeepAlive(iterator);
      return result;
    }

    public static explicit operator string(Iterator iterator)
    {
      uint len;
      var p = NativeMethods.bson_iter_utf8(iterator.it, out len);
      var result = Utils.StringFromNativeUtf8(p, len);
      GC.KeepAlive(iterator);
      return result;
    }

    public static explicit operator Oid(Iterator iterator)
    {
      /* Following temporary returned out of scope. Will get freed when collector calls finalized */
      var oid = Builder.newOid(NativeMethods.bson_iter_oid(iterator.it));
      GC.KeepAlive(iterator);
      return oid;
    }

    public static explicit operator double(Iterator iterator)
    {
      var result = NativeMethods.bson_iter_double(iterator.it);
      GC.KeepAlive(iterator);
      return result;
    }

    public static explicit operator Int64(Iterator iterator)
    {
      var result = NativeMethods.bson_iter_int64(iterator.it);
      GC.KeepAlive(iterator);
      return result;
    }

    public static explicit operator bool(Iterator iterator)
    {
      var result = NativeMethods.bson_iter_bool(iterator.it) == 1;
      GC.KeepAlive(iterator);
      return result;
    }

    public static explicit operator DateTime(Iterator iterator)
    {
      var result = Utils.ToDateTimeFromMillisecondsSinceEpoch(NativeMethods.bson_iter_date_time(iterator.it)).ToLocalTime();
      GC.KeepAlive(iterator);
      return result;
    }

    public static explicit operator byte[](Iterator iterator)
    {
      uint len;
      int subtype;
      IntPtr data;
      NativeMethods.bson_iter_binary(iterator.it, out subtype, out len, out data);

      var result = new byte[len];
      Marshal.Copy(data, result, 0, (int)len);
      GC.KeepAlive(iterator);
      return result;
    }

    public bool next()
    {
      var result = NativeMethods.bson_iter_next(it) != 0;
      GC.KeepAlive(this);
      return result;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
    public bool find(string Name)
    {
      while (next())
      {
        if (key == Name) return true;
      }
      return false;
    }
  }

  public class _Iterator : Iterator
  {
    public _Iterator()
    {
      it = NativeMethods.bson_iter_alloc();
      Utils.UnmanagedObjectTracker.Register(GetType(), it, NativeMethods.bson_iter_dealloc);
    }

    public _Iterator(Bson b)
      : this()
    {
      NativeMethods.bson_iter_init(it, b.Handle);
      Utils.UnmanagedObjectTracker.AddParent(GetType(), it, b.GetType(), b.Handle);
      GC.KeepAlive(b);
    }

    /* Copy constructor used to acquire a subiterator */
    public _Iterator(Iterator it)
      : this()
    {
      if (NativeMethods.bson_iter_recurse(it.it, this.it) != 1)
        throw new BsonException("Failed creating Iterator using bson_iter_recurse()");
      Utils.UnmanagedObjectTracker.AddParent(GetType(), this.it, it.GetType(), it.it);
      GC.KeepAlive(it);
    }

    /* Copy constructor used to acquire a subiterator using a dotkey name notation */
    public _Iterator(Iterator ait, string dotkey)
      : this()
    {
      if (NativeMethods.bson_iter_find_descendant(ait.it, dotkey, it) != 1)
        throw new BsonException("Failed creating subiterator using dotkey notation");
      Utils.UnmanagedObjectTracker.AddParent(GetType(), it, ait.GetType(), ait.it);
      GC.KeepAlive(ait);
    }
  }
}
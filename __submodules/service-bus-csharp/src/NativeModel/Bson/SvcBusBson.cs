using System;
using System.Runtime.InteropServices;
using GChelpers;

namespace Sovos.SvcBus
{
  using bson = IntPtr;

  public class Bson : ICloneable
  {
    public enum DisposalMethod { DontDispose, CallBsonDestroy, CallBsonFree }
    protected Bson() { }
 
    public bson Handle { get; protected set; }

    private void checkBsonResult(int result, string action)
    {
      if(result == 0) 
        throw new BsonException("Failure calling function: " + action);
    }

    protected void RegisterHandle(DisposalMethod disposalMethod)
    {
      UnmanagedObjectContext<Type, IntPtr>.DestroyHandleDelegate destroyDelegate;
      switch (disposalMethod)
      {
        case DisposalMethod.CallBsonDestroy:
          destroyDelegate = NativeMethods.bson_destroy;
          break;
        case DisposalMethod.CallBsonFree:
          destroyDelegate = NativeMethods.bson_free;
          break;
        default:
          destroyDelegate = null;
          break;
      }
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle, destroyDelegate);  
    }

    ~Bson()
    {
      if (Handle == bson.Zero)
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = bson.Zero;
    }

    public object Clone()
    {
      return new _Bson(this);
    }

    public string ToJson()
    {
      var buf = NativeMethods.bson_as_json(Handle, 0);
      var json = Marshal.PtrToStringAnsi(buf);
      NativeMethods.bson_free(buf);
      GC.KeepAlive(this);
      return json;
    }

    public void append(string name, int value)
    {
      checkBsonResult(NativeMethods.bson_append_int32(Handle, name, -1, value), "append(int)");
      GC.KeepAlive(this);
    }

    public void append(string name, string value)
    {
      IntPtr p = Utils.NativeUtf8FromString(value);
      try
      {
        checkBsonResult(NativeMethods.bson_append_utf8(Handle, name, -1, p, -1), "append(string)");
        GC.KeepAlive(this);
      }
      finally
      {
        Marshal.FreeHGlobal(p);
      }
    }

    public void append(string name, Oid oid)
    {
      checkBsonResult(NativeMethods.bson_append_oid(Handle, name, -1, oid.oid), "append(Oid)");
      GC.KeepAlive(oid);
      GC.KeepAlive(this);
    }

    public void append(string name, double value)
    {
      checkBsonResult(NativeMethods.bson_append_double(Handle, name, -1, value), "append(double)");
      GC.KeepAlive(this);
    }

    public void append(string name, Int64 value)
    {
      checkBsonResult(NativeMethods.bson_append_int64(Handle, name, -1, value), "append(Int64)");
      GC.KeepAlive(this);
    }

    public void append(string name, bool value)
    {
      checkBsonResult(NativeMethods.bson_append_bool(Handle, name, -1, (byte)(value ? 1 : 0)), "append(bool)");
      GC.KeepAlive(this);
    }

    public void append(string name, DateTime value)
    {
      checkBsonResult(NativeMethods.bson_append_date_time(Handle, name, -1, Utils.ToMillisecondsSinceEpoch(value)), "append(DateTime)");
      GC.KeepAlive(this);
    }

    public void append(string name, bson_binary_subtype_t type, byte[] value)
    {
      var p = Marshal.AllocHGlobal(value.Length);
      try
      {
        Marshal.Copy(value, 0, p, value.Length);
        checkBsonResult(NativeMethods.bson_append_binary(Handle, name, -1, (byte)type, p, (uint)value.Length), "append(byte[])");
        GC.KeepAlive(this);
      }
      finally
      {
        Marshal.FreeHGlobal(p);
      }
    }

    public void append(string name, bson_binary_subtype_t type, IntPtr value, uint size)
    {
      checkBsonResult(NativeMethods.bson_append_binary(Handle, name, -1, (int)type, value, size), "append(IntPtr)");
      GC.KeepAlive(this);
    }

    public Bson appendDocumentBegin(string name)
    {
      var sub = Builder.newBson(DisposalMethod.CallBsonFree);
      int res = NativeMethods.bson_append_document_begin(Handle, name, -1, sub.Handle);
      checkBsonResult(res, "appendDocumentBegin");
      Utils.UnmanagedObjectTracker.AddParent(sub.GetType(), sub.Handle, GetType(), Handle);
      GC.KeepAlive(this);
      return sub;
    }

    public void appendDocumentEnd(Bson sub)
    {
      checkBsonResult(NativeMethods.bson_append_document_end(Handle, sub.Handle), "appendDocumentEnd");
      Utils.UnmanagedObjectTracker.RemoveParent(sub.GetType(), sub.Handle, GetType(), Handle);
      GC.KeepAlive(sub);
      GC.KeepAlive(this);
    }

    public Bson appendArrayBegin(string name)
    {
      var sub = Builder.newBson(DisposalMethod.CallBsonFree);
      int res = NativeMethods.bson_append_array_begin(Handle, name, -1, sub.Handle);
      checkBsonResult(res, "appendArrayBegin");
      Utils.UnmanagedObjectTracker.AddParent(sub.GetType(), sub.Handle, GetType(), Handle);
      GC.KeepAlive(this);
      return sub;
    }

    public void appendArrayEnd(Bson sub)
    {
      checkBsonResult(NativeMethods.bson_append_array_end(Handle, sub.Handle), "appendArrayEnd");
      Utils.UnmanagedObjectTracker.RemoveParent(sub.GetType(), sub.Handle, GetType(), Handle);
      GC.KeepAlive(sub);
      GC.KeepAlive(this);
    }

    public void appendNull(string name)
    {
      checkBsonResult(NativeMethods.bson_append_null(Handle, name, -1), "appendNull");
      GC.KeepAlive(this);
    }

    public void appendMaxKey(string name)
    {
      checkBsonResult(NativeMethods.bson_append_maxkey(Handle, name, -1), "appendMaxKey");
    }

    public void appendMinKey(string name)
    {
      checkBsonResult(NativeMethods.bson_append_minkey(Handle, name, -1), "appendMinKey");
      GC.KeepAlive(this);
    }

    public Iterator find(string name)
    {
      /* Object returned outside of an scope */
      if (name.Contains("."))
      {
        var it = Builder.newIterator(this); 
        try
        {
          return Builder.newIterator(it, name);
        }
        catch (BsonException)
        {
          return null;
        }
      }
      var _it = Builder.newIterator();
      var result = NativeMethods.bson_iter_init_find(_it.it, Handle, name) == 1 ? _it : null;
      GC.KeepAlive(_it);
      GC.KeepAlive(this);
      return result;
    }
  }

  internal class _Bson : Bson
  {
    public _Bson(bson b)
    {
      Handle = b;
      RegisterHandle(DisposalMethod.DontDispose);
    }

    public _Bson(DisposalMethod disposalMethod)
    {
      Handle = NativeMethods.bson_new();
      RegisterHandle(disposalMethod);
    }

    public _Bson(Bson srcBson)
    {
      Handle = NativeMethods.bson_copy(srcBson.Handle);
      GC.KeepAlive(srcBson);
      RegisterHandle(DisposalMethod.CallBsonDestroy);
    }
  }
}

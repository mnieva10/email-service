using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sovos.SvcBus
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
  public class Oid: ICloneable
  {
    private readonly IntPtr _oid;
    private int _hash;

    public Oid()
    {
      _oid = NativeMethods.ServiceBusOid_alloc();
    }

    public Oid(IntPtr oid) : this()
    {
      this.oid = oid;
    }

    public int Int(int index)
    {
      int[] oidArray = new int[3];
      Marshal.Copy(_oid, oidArray, index, 1);
      GC.KeepAlive(this);
      return oidArray[index];
    }

    public IntPtr oid
    {
      get
      {
        return _oid;
      }
      protected set
      {
        NativeMethods.bson_oid_copy(value, _oid);
        GC.KeepAlive(this);
        Rehash();
      }
    }

    private void Rehash()
    {
      _hash = ToString().GetHashCode();
    }

    private int GetHash()
    {
      return _hash;
    }

    ~Oid()
    {
      NativeMethods.ServiceBusOid_dealloc(_oid);
    }

    public void Gen()
    {
      NativeMethods.bson_oid_init(_oid);
      GC.KeepAlive(this);
      Rehash();
    }

    public static bool operator ==(Oid oid1, Oid oid2)
    {
      if (ReferenceEquals(oid1, oid2))
      {
        // handles if both are null as well as object identity
        return true;
      }

      var result = !ReferenceEquals(null, oid1) && !ReferenceEquals(null, oid2) && NativeMethods.SvcBus_isBsonOidEqual(oid1.oid, oid2.oid) == 1;
      GC.KeepAlive(oid1);
      GC.KeepAlive(oid2);
      return result;
    }

    public static bool operator !=(Oid oid1, Oid oid2)
    {
      return !(oid1 == oid2);
    }

    protected bool Equals(Oid other)
    {
      var result = this == other;
      GC.KeepAlive(other);
      GC.KeepAlive(this);
      return result;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) 
        return false;
      if (ReferenceEquals(this, obj)) 
        return true;
      var result = obj.GetType() == GetType() && Equals((Oid)obj);
      GC.KeepAlive(obj);
      GC.KeepAlive(this);
      return result;
    }

    public override int GetHashCode()
    {
      return GetHash();
    }

    public override string ToString()
    {
      var res = new StringBuilder(25);
      NativeMethods.bson_oid_to_string(oid, res);
      GC.KeepAlive(this);
      return res.ToString();
    }

    public object Clone()
    {
      var res = Builder.newOid();
      NativeMethods.bson_oid_init_from_string(res.oid, ToString());
      GC.KeepAlive(this);
      res.Rehash();
      return res;
    }
  }
}
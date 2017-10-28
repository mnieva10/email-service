using System.Threading;

namespace GChelpers
{
  public class UnmanagedObjectContext<THandleClass, THandle>
  {
    public delegate void DestroyHandleDelegate(THandle obj);

    private int _refCount = 1;
    public DestroyHandleDelegate DestroyHandle { get; set; }
    public HandleCollection<THandleClass, THandle> parentCollection { get; set; }

    public void CallDestroyHandleDelegate(THandle obj)
    {
      if (DestroyHandle != null)
        DestroyHandle(obj);
    }

    public int AddRefCount()
    {
      return Interlocked.Increment(ref _refCount);
    }

    public int ReleaseRefCount()
    {
      return Interlocked.Decrement(ref _refCount);
    }

    public void InitParentCollection()
    {
      parentCollection = new HandleCollection<THandleClass, THandle>();
    }
  }
}
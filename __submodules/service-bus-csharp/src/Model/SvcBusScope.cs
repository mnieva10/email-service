/** 
 * Use Builder class directly to build OIDs, Bsons, Iterators and Messages. The main purpose of using the builder directly is to create
 * temporaries. Be careful to define the scope of the temporary using the keyword "using(var temporary = Builder.newWhateverObject(...)) { ... } 
 
 * For other type of objects, use an object of class Scope so they have clearly defined scope and deterministic finalization rules to prevent
 * access violations because of out of order freeing of memory.
 
 * When dealing with unmanaged objects, such as the ones on ServiceBus library, that have dependencies between them (Consumers need PipeService, etc.)
 * a deterministic memory finalization model is required.
 * .NET garbage collector doesn't guaranteeing neither timing nor order of finalization of objects, something that caused problems on a first 
 * prototype of this library with random access violation errors. 
**/

using System;
using System.Collections.Generic;

namespace Sovos.SvcBus
{
  using bson_oid_p = IntPtr;
  using bson = IntPtr;
  using SvcBusResponder = IntPtr;
  using SvcBusConsumer = IntPtr;
  using SvcBusConsumerPool = IntPtr;
  using SvcBusService = IntPtr;

  [Serializable]
  public class EScope : Exception
  {
    public EScope(string msg) : base(msg) {}
  }

  public class Builder
  {
    public static Oid newOid()
    {
      return new Oid();
    }

    public static Oid newOid(bson_oid_p oid)
    {
      return new Oid(oid);
    }

    public static Bson newBson(Bson.DisposalMethod disposalMethod = Bson.DisposalMethod.CallBsonDestroy)
    {
      return new _Bson(disposalMethod);
    }

    public static Bson newBson(bson b)
    {
      return new _Bson(b);
    }

    public static Iterator newIterator()
    {
      return new _Iterator();
    }

    public static Iterator newIterator(Bson b)
    {
      return new _Iterator(b);
    }

    /* Builder constructor used to acquire a subiterator */
    public static Iterator newIterator(Iterator it)
    {
      return new _Iterator(it);
    }

    /* Builder constructor used to acquire a subiterator using dotkey name notation */
    public static Iterator newIterator(Iterator it, string dotkey)
    {
      return new _Iterator(it, dotkey);
    }

    public static Message newMessage(bson_oid_p _oid)
    {
      return new _Message(_oid);
    }

    public static Message newMessage(Oid _oid)
    {
      return new _Message(_oid);
    }

    public static Message newMessage(Message.InitMode initMode)
    {
      return new _Message(initMode);
    }

    public static Message newMessage(Message srcMessage)
    {
      return new _Message(srcMessage as _Message);
    }

    public static Message newMessage()
    {
      return new _Message();
    }

    public static Consumer newConsumer(PipeService ps, string name, Service svc, svc_bus_consumer_options_t options = 0)
    {
      return new _Consumer(ps, name, svc, options);
    }

    public static Consumer newConsumer(PipeService ps, string name, string responsePipeName, svc_bus_consumer_options_t options = 0)
    {
      return new _Consumer(ps, name, responsePipeName, options);
    }

    public static Consumer newConsumer(ConsumerPool pool, SvcBusConsumer handle)
    {
      return new _Consumer(pool, handle);
    }

    public static Service newService(string name, svc_bus_queue_mode_t mode, uint volatileSize = 0, uint responsePipeSize = 0)
    {
      return new _Service(name, mode, volatileSize, responsePipeSize);
    }

    public static Service newService(SvcBusService handle)
    {
      return new _Service(handle);
    }

    public static PipeService newPipeService(string connectionString)
    {
      return new _PipeService(connectionString);
    }

    public static ConsumerPool newConsumerPool(PipeService ps, string name, Service service, int options = 0)
    {
      return new _ConsumerPool(ps, name, service, options);
    }

    public static ConsumerPool newConsumerPool(SvcBusConsumerPool consumerPoolHandle)
    {
      return new _ConsumerPool(consumerPoolHandle);
    }

    public static Producer newProducer(PipeService ps, string name, Service svc)
    {
      return new _Producer(ps, name, svc);
    }

    public static ServicePersistence newServicePersistence(PipeService ps)
    {
      return new _ServicePersistence(ps);
    }

    public static BaseBsonSerializer newBsonSerializer(Type targetType, object source, Bson target)
    {
      return BaseBsonSerializer.CreateSerializer(targetType, new object[] { source, target });
    }

    public static BaseBsonSerializer newBsonSerializer(Type targetType)
    {
      return BaseBsonSerializer.CreateSerializer(targetType);
    }

    public static DispatchInterfaceConsumer newDispatchInterfaceConsumer(PipeService ps, string name, Service svc, svc_bus_consumer_options_t options = 0)
    {
      return new DispatchInterfaceConsumer(ps, name, svc, options);
    }

    public static DispatchInterfaceConsumer newDispatchInterfaceConsumer(ConsumerPool consumerPool)
    {
      return new DispatchInterfaceConsumer(consumerPool);
    }

    public static Mutex newMutex(PipeService ps, string name, string svcname)
    {
      return new _Mutex(ps, name, svcname);
    }

    public static Mutex newMutex(PipeService ps)
    {
      return new _Mutex(ps);
    }

    public static Responder newResponder(SvcBusResponder responder)
    {
      return new _Responder(responder);
    }

    public static Responder newResponder(PipeService ps)
    {
      return new _Responder(ps);
    }

    public static Dispatcher newDispatcher(string name, Service svc, PipeService ps, Type dispatchInterfaceClass)
    {
      return new Dispatcher(name, svc, ps, dispatchInterfaceClass);
    }

    public static GenericErr newGenericErr()
    {
      return new GenericErr();
    }

    public static ConsumerPoolDictionary newConsumerPoolDictionary()
    {
      return new _ConsumerPoolDictionary();
    }

    public static DispatchInterfaceConsumerPool newDispatchInterfaceConsumerPool(PipeService ps, Service service, string name)
    {
      return new DispatchInterfaceConsumerPool(ps, service, name);
    }
  }

  public class Scope : IDisposable
  {
    private readonly List<IDisposable> _objs = new List<IDisposable>();

    public Scope() {}

    public Scope(IDisposable[] objects)
    {
      foreach (var obj in objects)
      {
        if(obj != null)
          add(obj);
      }
    }
    
    public Consumer newConsumer(PipeService ps, string name, Service svc, svc_bus_consumer_options_t options = 0)
    {
      return add(Builder.newConsumer(ps, name, svc, options));
    }

    public Consumer newConsumer(PipeService ps, string name, string responsePipeName, svc_bus_consumer_options_t options = 0)
    {
      return add(Builder.newConsumer(ps, name, responsePipeName, options));
    }

    public Producer newProducer(PipeService ps, string name, Service svc)
    {
      return add(Builder.newProducer(ps, name, svc));
    }

    public PipeService newPipeService(string connectionString)
    {
      return add(Builder.newPipeService(connectionString));
    }

    public Dispatcher newDispatcher(string name, Service svc, PipeService ps, Type dispatchInterfaceClass)
    {
      return add(Builder.newDispatcher(name, svc, ps, dispatchInterfaceClass));
    }

    public Dispatcher newDispatcher(Service svc, PipeService ps, Type dispatchInterfaceClass)
    {
        var dispatcherInstanceId = dispatchInterfaceClass.Name.Replace("DispatchInterface", "Dispatcher");

        dispatcherInstanceId += string.Format("-{0}-PID:{1}", Environment.MachineName, System.Diagnostics.Process.GetCurrentProcess().Id);

        return add(Builder.newDispatcher(dispatcherInstanceId, svc, ps, dispatchInterfaceClass));
    }

    public DispatchInterfaceConsumer newDispatchInterfaceConsumer(PipeService ps, string name, Service svc, svc_bus_consumer_options_t options = 0)
    {
      return add(Builder.newDispatchInterfaceConsumer(ps, name, svc, options));
    }

    public DispatchInterfaceConsumer newDispatchInterfaceConsumer(ConsumerPool consumerPool)
    {
      return add(Builder.newDispatchInterfaceConsumer(consumerPool));
    }

    public ConsumerPool newConsumerPool(PipeService ps, string name, Service service, int options = 0)
    {
      return add(Builder.newConsumerPool(ps, name, service, options));
    }

    public ConsumerPool newConsumerPool(SvcBusConsumerPool consumerPoolHandle)
    {
      return add(Builder.newConsumerPool(consumerPoolHandle));
    }

    public ConsumerPoolDictionary newConsumerPoolDictionary()
    {
      return add(Builder.newConsumerPoolDictionary());
    }

    public DispatchInterfaceConsumerPool newDispatchInterfaceConsumerPool(PipeService ps, Service service, string name)
    {
      return add(Builder.newDispatchInterfaceConsumerPool(ps, service, name));
    }

    public Mutex newMutex(PipeService ps, string name, string svcname)
    {
      return add(Builder.newMutex(ps, name, svcname));
    }

    public Mutex newMutex(PipeService ps)
    {
      return add(Builder.newMutex(ps));
    }
    
    public Consumer add(Consumer consumer)
    {
      _objs.Add(consumer);
      return consumer;
    }

    public Producer add(Producer producer)
    {
      _objs.Add(producer);
      return producer;
    }

    public PipeService add(PipeService pipeService)
    {
      _objs.Add(pipeService);
      return pipeService;
    }

    public Dispatcher add(Dispatcher dispatcher)
    {
      _objs.Add(dispatcher);
      return dispatcher;
    }

    public IDisposable add(IDisposable obj)
    {
      _objs.Add(obj);
      return obj;
    }

    public Mutex add(Mutex obj)
    {
      _objs.Add(obj);
      return obj;
    }
    
    public ConsumerPool add(ConsumerPool obj)
    {
      _objs.Add(obj);
      return obj;
    }

    public ConsumerPoolDictionary add(ConsumerPoolDictionary obj)
    {
      _objs.Add(obj);
      return obj;
    }

    public DispatchInterfaceConsumerPool add(DispatchInterfaceConsumerPool obj)
    {
      _objs.Add(obj);
      return obj;
    }

    public DispatchInterfaceConsumer add(DispatchInterfaceConsumer obj)
    {
      _objs.Add(obj);
      return obj;
    }

    ~Scope()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing) return;
      for (var i = _objs.Count - 1; i >= 0; i--)
        if (_objs[i] != null)
          _objs[i].Dispose();
      _objs.Clear(); /* let's make sure to remove all references to object so a second call doesn't double dispose objects */
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }
}

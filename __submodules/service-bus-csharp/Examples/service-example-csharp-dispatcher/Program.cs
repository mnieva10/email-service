using System;
using System.IO;
using System.Threading;
using Sovos.SvcBus;

namespace service_example_csharp_dispatcher
{
  internal class EchoRequest
  {
    [SvcBusSerializable]
    public bool RequestBinData { get; set; }
    [SvcBusSerializable]
    public string EchoField { get; set; }
  }

  internal class EchoResponse
  {
    [SvcBusSerializable]
    public MemoryStream BinData { get; private set; }
    [SvcBusSerializable]
    public string EchoField { get; set; }

    public EchoResponse()
    {
      BinData = new MemoryStream();
    }
  }

  public class TestServiceDispatchInterface : DispatchInterface
  {
    private int _counter;

    public TestServiceDispatchInterface(object userData) : base(userData) { }
    public object Produce(Message msg)
    {
      if (Interlocked.Increment(ref _counter) == 1000)
      {
        Console.WriteLine("Taken 1000 more");
        Interlocked.Exchange(ref _counter, 0);
        GC.Collect();
      }

      var request = (EchoRequest)DeserializeRequest(msg);

      var response = new EchoResponse { EchoField = request.EchoField };
      if (request.RequestBinData)
        response.BinData.Write(Program.staticBinData, 0, Program.staticBinData.Length);
      return response;
    }
  }

  class Program
  {
    private static readonly Scope Scope = new Scope();
    private const int BinDataSize = 1024 * 100;
    public static byte[] staticBinData;

    private static void StartConsuming(PipeService ps, Service service)
    {
      var messagesSent = 0;
      var dic = Scope.newDispatchInterfaceConsumer(ps, "consumer", service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      var start = (uint)Environment.TickCount;
      dic.Consumer.startUpdateHeartbeatTimer(1000);
      /* start sending requests */
      while (true)
      {
        using (var responseReceivedEvent = new AutoResetEvent(false))
        {

          var request = new EchoRequest
          {
            EchoField = "test", 
            RequestBinData = staticBinData != null
          };
          var requestMessage = dic.BuildRequestMessage("Produce", request);

          dic.WaitResponseAsync(null, requestMessage, null, (response, requestMsg, replier, userdata) =>
          {
            // ReSharper disable once AccessToDisposedClosure
            responseReceivedEvent.Set();
            try
            {
              /* First thing we do is to check if our Bson object contains an exception */
              BsonDeserializer.CheckSvcBusException(response);
              /* Deserialize the response, that was provided to use as a Bson into a native EchoResponse object*/
              var responseObj = (EchoResponse)DispatchInterface.DeserializeResponse(response);
              /* userdata has our source request object */
              var req = (EchoRequest) userdata;

              if (responseObj.EchoField != req.EchoField)
                Console.WriteLine("echo received {0} when sent {1}", responseObj.EchoField, req.EchoField);
              if (staticBinData != null && staticBinData.Length != responseObj.BinData.Length) // avoid memory compare
                Console.WriteLine("Contents of bin_data field doesn't match expected data");
            }
            catch (ConsumerException e)
            {
              if (e.ErrorCode == svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED)
                Console.WriteLine("timeout waiting for response");
              else
                Console.WriteLine("error [{0}] waiting for response", e.Message);
            }
            catch (Exception e)
            {
              Console.WriteLine("Fatal error [{0}] waiting for response!!", e.Message);
            }
          }, request);

          dic.Send(requestMessage);

          var currentTime = (uint) Environment.TickCount;
          var timeDiff = (uint) Math.Abs((long) currentTime - start);
          messagesSent++;
          if (messagesSent >= (service.Mode == svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE ? 1000u : 10u) && timeDiff > 0)
          {
            Console.WriteLine("{0} inserted, avg speed {1} requests per second", messagesSent,
              (long) (messagesSent/((double) timeDiff/1000)));
            messagesSent = 0;
            start = (uint) (Environment.TickCount);
          }

          responseReceivedEvent.WaitOne();
        }
      }
      // ReSharper disable once FunctionNeverReturns
    }

    private static void Main(string[] args)
    {
      if (args.Length < 3)
      {
        Console.WriteLine("USAGE: CONNECTION_STRING SERVICE_NAME DURABLE|VOLATILE [producer] [use_bin_data]");
        return;
      }

      var ps = Scope.newPipeService(args[0]);
      ps.startAsyncWorkPool(2, 2);
      var svc = Builder.newService(args[1], args[2] == "DURABLE" ? svc_bus_queue_mode_t.SERVICE_BUS_DURABLE : svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
      var dispatcher = Scope.newDispatcher("service producer", svc, ps, typeof (TestServiceDispatchInterface));
      dispatcher.Logger = new SimpleConsoleLogger();

      if ((args.Length > 3 && args[3] == "use_bin_data") || (args.Length > 4 && args[4] == "use_bin_data"))
      {
        staticBinData = new byte[BinDataSize];
        for (var i = 0; i < BinDataSize; i++)
          staticBinData[i] = (byte)(i % 256);
      }

      var map = BuildableSerializableTypeMapper.Mapper;
      map.Register("EchoRequest", typeof(EchoRequest));
      map.Register("EchoResponse", typeof(EchoResponse));
      if (args.Length > 3 && args[3] == "producer")
      {
        dispatcher.Start(4, DispatchMode.SingletonDispatchInterface, staticBinData);

        while (true)
          Thread.Sleep(1000);
      }
      else
        StartConsuming(ps, svc);
      map.Unregister("EchoRequest");
      map.Unregister("EchoResponse");
    }
  }
}
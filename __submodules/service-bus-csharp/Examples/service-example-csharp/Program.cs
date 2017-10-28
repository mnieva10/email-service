using System;
using Sovos.SvcBus;

namespace service_example_csharp
{
  class Program
  {
    private static readonly Scope Scope = new Scope();
    private const int BinDataSize = 1024 * 100;
    static readonly byte[] BinData = new byte[BinDataSize];

    static void StartConsuming(PipeService ps, Service service, bool waitForResponse, bool waitBinData)
    {
      var messagesSent = 0;
      var consumer = Scope.newConsumer(ps, "consumer", service, waitForResponse ? svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE : 0);
      var start = (uint)Environment.TickCount;
      consumer.startUpdateHeartbeatTimer(1000);
      /* start sending requests */
      while (true)
      {
          var msg = Builder.newMessage(Message.InitMode.inited);
          msg.bson.append("data", 111);
          consumer.send(msg);
          var currentTime = (uint) Environment.TickCount;
          var timeDiff = (uint) Math.Abs((Int64) currentTime - start);
          messagesSent++;
          if (messagesSent >= (service.Mode == svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE ? 1000u : 10u) && timeDiff > 0)
          {
            GC.Collect(); // Without this the GC never kicks eating memory like crazy
            Console.WriteLine("{0} inserted, avg speed {1} requests per second", messagesSent, (long) (messagesSent/((double) timeDiff/1000)));
            messagesSent = 0;
            start = (uint) (Environment.TickCount);
          }

          if (!waitForResponse) continue;
        try
        {
          var response = consumer.wait(msg.messageId);
          var it = response.find("msgId");
          if (it.bsonType == bson_type.BSON_EOO)
          {
            Console.WriteLine("msgId field not found in response");
            continue;
          }
          if ((Oid) it != msg.messageId)
          {
            Console.WriteLine("OID of request is different than of received response");
            continue;
          }
          if (!waitBinData) continue;
          it = response.find("bin_data");
          if (it.bsonType == bson_type.BSON_BINDATA)
          {
            var binDataRead = (byte[]) it;
            if (BinData.Length != binDataRead.Length)
            {
              Console.WriteLine(
                "Number of binary elements received doesn't match expected number of elements. Received {0} bytes",
                binDataRead.Length);
              continue;
            }
            for (var i = 0; i < BinData.Length; i++)
            {
              if (BinData[i] == binDataRead[i]) continue;
              Console.WriteLine("Contents of bin_data field doesn't match expected data at position {0}", i);
              break;
            }
          }
          else
          {
            Console.WriteLine("Expected bin_data field but was not present");
          }
        }
        catch (ConsumerException e)
        {
          if (e.ErrorCode == svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED)
          {
            Console.WriteLine("timeout waiting for response");
          }
          else
          {
            throw;
          }
        }
      }
    }

    static void StartProducing(PipeService ps, Service service, bool sendBinData)
    {
      var counter = 0;
      var producer = Scope.newProducer(ps, "producer", service);
      ps.releaseDeadResponsePipes(120 * 1000);
      while (true)
      {
        var msg = producer.wait();
        if (!producer.take(msg)) continue;
        if (counter++ == 1000)
        {
          GC.Collect();
          Console.WriteLine("Taken 1000 more");
          counter = 0;
        }
        var response = Builder.newMessage(msg.messageId);
        response.bson.append("testField", "this is a response");
        if (sendBinData)
        {
          response.bson.append("bin_data", bson_binary_subtype_t.BSON_BIN_BINARY, BinData);
        }
        producer.responder.send(msg.responsePipeName, response);
      }
      // ReSharper disable once FunctionNeverReturns
    }

    static void Main(string[] args)
    {
      if (args.Length < 3)
      {
        Console.WriteLine("USAGE: CONNECTION_STRING SERVICE_NAME DURABLE(VOLATILE) [producer]");
        return;
      }

      var ps = Scope.newPipeService(args[0]);
      ps.startAsyncWorkPool(2, 2);
      var svc = Builder.newService(args[1], args[2] == "DURABLE" ? svc_bus_queue_mode_t.SERVICE_BUS_DURABLE : svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);

      for (var i = 0; i < BinDataSize; i++)
      {
        BinData[i] = (byte) (i % 256);
      }

      var useBinData = args.Length > 4 && args[4] == "use_bin_data";

      if (args.Length > 3 && args[3] == "producer")
      {
        StartProducing(ps, svc, useBinData);
      }
      else
      {
        StartConsuming(ps, svc, args.Length > 3 && args[3] == "wait", useBinData);
      }
    }
  }
}

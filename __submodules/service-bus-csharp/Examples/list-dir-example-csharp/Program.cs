using System;
using Sovos.SvcBus;
using System.Collections.Generic;
using System.IO;

namespace list_dir_example_csharp
{
  class Program
  {
    private static readonly Scope Scope = new Scope();

    private static void StartConsuming( PipeService ps, Service service )
    {
      var consumer = Scope.newConsumer(ps, "list_dir consumer", service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.startUpdateHeartbeatTimer(1000);
      while (true)
      {
          /* get path from stdin */
          Console.Write("Type dir path(or \"exit\"): ");
          string path;

          if ((path = Console.ReadLine()) == "exit") break;
          /* Notice here the scope of variable msg clearly defined to force deallocation of unmanaged memory */
          var msg = Builder.newMessage(Message.InitMode.inited);
          /* set custom data */
          msg.bson.append("path", path);
        try
        {
          /* Notice here the scope of variable response clearly defined to force deallocation of unmanaged memory */
          var response = consumer.sendAndWait(msg);
          /* Notice here the scope of variable it clearly defined to force deallocation of unmanaged memory */
          var it = response.find("status");
          Console.WriteLine((string) it == "ok" ? response.ToJson() : "Invalid path");
        }
        catch (SvcBusException e)
        {
          Console.WriteLine(e.Message);
        }
      }
    }

    /* appends array on file names from folder specified by path to result bson 
       returns true if path valid, otherwise false */

    private static bool DirToBson(string path, Bson result)
    {      
      var i = 0;
      List<string> dirs;
      try
      {
        dirs = new List<string>(Directory.EnumerateFileSystemEntries(path));
      }
      catch(Exception)
      {
        return false;
      }
      var sub = result.appendArrayBegin("dir");
      foreach (var dir in dirs)
        sub.append((++i).ToString(), Path.GetFileName(dir));
      result.appendArrayEnd(sub);  
      return true;
    }

    private static void StartProducing( PipeService ps, Service service )
    {
      var producer = Scope.newProducer(ps, "list_dir producer", service);
      ps.releaseDeadResponsePipes(NativeMethods.SERVICE_BUS_DEFAULT_HEARTBEAT_LIMIT_MS);
      while (true)
      {
        try
        {
          var requestMsg = producer.wait();
          if (!producer.take(requestMsg)) 
            continue;
          var responseMsg = Builder.newMessage(requestMsg.messageId);
          var it = requestMsg.bson.find("path");
          /* set response */
          var pathIsValid = DirToBson((string) it, responseMsg.bson);
          responseMsg.bson.append("status", pathIsValid ? "ok" : "fail");
          producer.responder.send(requestMsg.responsePipeName, responseMsg);
        }
        catch (SvcBusException e)
        {
          Console.WriteLine(e.Message);
        }
      }
    }

    private static void Main(string[] args)
    {
      if( args.Length == 0 )
      {
        Console.WriteLine( "USAGE: CONNECTION_STRING [producer]" );
        return;
      }
      var ps = Scope.newPipeService(args[0]);
      try
      {
        ps.startAsyncWorkPool(2, 2);
        var service = Builder.newService("list_dir", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
        if (args.Length > 1 && args[1] == "producer")
          StartProducing(ps, service);
        else
          StartConsuming(ps, service);
      }
      finally
      {
        Scope.Dispose();
      }
    }
  }
}

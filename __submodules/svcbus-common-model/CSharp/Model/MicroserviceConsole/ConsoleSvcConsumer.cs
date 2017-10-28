using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole
{
    public static class ConsoleSvcConsumer
    {
        private static int messagesSent = 0;
        private static uint start = (uint)Environment.TickCount;
        private static MessageBuilder msgBuilder = new MessageBuilder();
        public const int BufSize = 8192;

        public static void StartConsuming(Scope scope, PipeService ps, Service service)
        {
            var dic = scope.newDispatchInterfaceConsumer(ps, "consumer", service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
            dic.Consumer.startUpdateHeartbeatTimer(60000);
            dic.Consumer.ResponseTimeout = 60000;

            while (true)
            {
                try
                {
                    Console.WriteLine("-------");
                    Console.Write("Type command arguments or /h for help: ");
#if !NETCORE
                    var inStream = Console.OpenStandardInput(BufSize);
#else
                    var inStream = Console.OpenStandardInput();
#endif
                    Console.SetIn(new StreamReader(inStream, Console.InputEncoding, false, BufSize));

                    string input;
                    if ((input = Console.ReadLine()) == "exit") break;

                    var request = new object();

                    var arguments = new List<string> { "Describe" };
                    if (!string.IsNullOrEmpty(input) && input != "/h")
                    {
                        arguments = input.SplitCommandLine().ToList();
                        var msg = msgBuilder.Build(arguments);
                        request = msgBuilder.RequestObject;
                    }

                    var command = arguments[0];
                    var requestId = dic.Send(command, request);
                    GCollect();
                    var respObj = dic.Wait(requestId);

                    Console.WriteLine(command == "Describe" ? ((DataDto)respObj).DataString : new JsonFormatter().Print(respObj));
                }
                catch (ConsumerException ex)
                {
                    Console.WriteLine(ex.ErrorCode == svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED ? "timeout waiting for response" : ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void Consume(Scope scope, PipeService ps, Service service, string[] args)
        {
            var dic = scope.newDispatchInterfaceConsumer(ps, "consumer", service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
            dic.Consumer.startUpdateHeartbeatTimer(60000);

            try
            {
                var request = new object();

                var arguments = new List<string> { "Describe" };
                if (args.Length != 0 )
                {
                    arguments = args.ToList();
                    var msg = msgBuilder.Build(arguments);
                    request = msgBuilder.RequestObject;
                }

                var command = arguments[0];
                var requestId = dic.Send(command, request);
                GCollect();
                var respObj = dic.Wait(requestId);
                Console.WriteLine(command == "Describe" ? ((DataDto)respObj).DataString : new JsonFormatter().Print(respObj));
            }
            catch (ConsumerException ex)
            {
                Console.WriteLine(ex.ErrorCode == svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED ? "timeout waiting for response" : ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Consume(Scope scope, PipeService ps, Service service, string command, object request)
        {
            var dic = scope.newDispatchInterfaceConsumer(ps, "consumer", service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
            dic.Consumer.startUpdateHeartbeatTimer(60000);

            try
            {
                var requestId = dic.Send(command, request);
                GCollect();
                var respObj = dic.Wait(requestId);
                Console.WriteLine(command == "Describe" ? ((DataDto)respObj).DataString : new JsonFormatter().Print(respObj));
            }
            catch (ConsumerException ex)
            {
                Console.WriteLine(ex.ErrorCode == svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED ? "timeout waiting for response" : ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void GCollect()
        {
            var currentTime = (uint)Environment.TickCount;
            var timeDiff = (uint)Math.Abs((Int64)currentTime - start);
            messagesSent++;
            if (messagesSent < 1000 || timeDiff <= 0) return;
            GC.Collect();
            Console.WriteLine("{0} inserted, avg speed {1} requests per second", messagesSent, (long)(messagesSent / ((double)timeDiff / 1000)));
            messagesSent = 0;
            start = (uint)(Environment.TickCount);
        }
    }
}

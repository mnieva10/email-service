using System;
using System.IO;
using System.Web.Script.Serialization;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Capability;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole
{
    public class ConsoleMicroservice
    {
        public const string NewCommandPromt = "Dispatcher > ";
        public const int BufSize = 8192;

        public void Start(string dispatcherName, ICmdInterpreter inrprtr, ILogger fileLogger)
        {
            //Console.WriteLine("Welcome to {0} Dispatcher", dispatcherName);
            while (true)
            {
                Console.Write("\n{0}", NewCommandPromt);

                //Increase default buffer size of ReadLine
#if !NETCORE
                var inStream = Console.OpenStandardInput(BufSize);
#else
                var inStream = Console.OpenStandardInput();
#endif
                Console.SetIn(new StreamReader(inStream, Console.InputEncoding, false, BufSize));

                var input = Console.ReadLine();
                if (input == null) continue;
                try
                {
                    switch (input.ToLower())
                    {
                        case "/?":
                        case "/h":
                        case "h":
                            Console.WriteLine("Supported commands:\n  Stop\n {0}", inrprtr.ProcessCommand("Help"));
                            break;
                        case "stop":
                            Console.WriteLine("Service is gracefully stopping...");
                            fileLogger.WriteLogEntry(LogLevel.Always, "Service is gracefully stopping...");
                            return;
                        default:
                            var obj = inrprtr.ProcessCommand(input);

                            if (obj is Message)
                                obj = new JavaScriptSerializer().Deserialize<BsonResponse>(((Message) obj).bson.ToJson());

                            Console.WriteLine(new JsonFormatter().Print(obj));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    fileLogger.WriteLogEntry(LogLevel.Error, string.Format("Caught exception in ConsoleMicroservice.cs: {1}\t{0}{1}\t{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                }
            }
        }

        public void Start(string dispatcherName, ICmdInterpreter inrprtr, string[] args)
        {
            try
            {
                var obj = inrprtr.ProcessCommand(string.Join(" ", args));
                Console.WriteLine(new JsonFormatter().Print(obj));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
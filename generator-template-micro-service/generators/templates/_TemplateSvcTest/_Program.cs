using System;
using System.Configuration;
using Sovos.Crypt.Model.Services;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.MicroserviceConsole;

namespace <%= test %>
{
    class Program
    {
        static void Main()
        {
            try
            {
                var mongoDbConnString = LoginUtility.Decrypt(ConfigurationManager.AppSettings["MongoDB.ConnectionString"]);
                var minThreads = Convert.ToUInt32(ConfigurationManager.AppSettings["MinThreads"]);
                var maxThreads = Convert.ToUInt32(ConfigurationManager.AppSettings["MaxThreads"]);
                var serviceName = ConfigurationManager.AppSettings["ServiceName"];
                var queueMode = ConfigurationManager.AppSettings["QueueMode"].ToUpper();

                var scope = new Scope();
                var ps = scope.newPipeService(mongoDbConnString);
                ps.startAsyncWorkPool(minThreads, maxThreads);
                var svc = Builder.newService(serviceName, queueMode == "DURABLE" ? svc_bus_queue_mode_t.SERVICE_BUS_DURABLE : svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);

                ConsoleSvcConsumer.StartConsuming(scope, ps, svc);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }
    }
}

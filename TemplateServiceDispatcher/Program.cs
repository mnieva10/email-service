using System;
using System.Configuration;
using System.Diagnostics;
using Sovos.Crypt.Model.Services;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Infrastructure.Logging;
using Sovos.SvcBus.Common.Model.MicroserviceConsole;
using Sovos.SvcBus.Common.Model.Strategies;
using Sovos.Template.Model.Capability;
using Sovos.Template.Model.Repositories;
using Sovos.Template.Model.Services;
using Sovos.Template.Persistence;

namespace TemplateServiceDispatcher
{
    class Program
    {
        static void Main()
        {
            using (var scope = new Scope())
            {
                FileLogger logger = null;
                try
                {
                    logger = new FileLogger((FileLoggerConfig)ConfigurationManager.GetSection("fileLoggerConfig"));
                    scope.add(logger);
                    logger.LogSettings(ConfigurationManager.AppSettings);

                    var mongoDbConnString = LoginUtility.Decrypt(ConfigurationManager.AppSettings["MongoDB.ConnectionString"]);
                    var minThreads = Convert.ToUInt32(ConfigurationManager.AppSettings["MinThreads"]);
                    var maxThreads = Convert.ToUInt32(ConfigurationManager.AppSettings["MaxThreads"]);
                    var serviceName = ConfigurationManager.AppSettings["ServiceName"];
                    var queueMode = ConfigurationManager.AppSettings["QueueMode"].ToUpper();
                    var ibatisConfig = ConfigurationManager.AppSettings["iBatis.Config"];
                    var ibatisConnString = LoginUtility.Decrypt(ConfigurationManager.AppSettings["iBatis.ConnectionString"]);
                    var threadCount = Convert.ToUInt32(ConfigurationManager.AppSettings["ThreadCount"]);
                    var dispatchMode = ConfigurationManager.AppSettings["DispatchMode"].ToUpper();
                    var myParameter = Convert.ToInt32(ConfigurationManager.AppSettings["MyParameter"]);

                    var ps = scope.newPipeService(mongoDbConnString);
                    ps.startAsyncWorkPool(minThreads, maxThreads);
                    var svc = Builder.newService(serviceName, queueMode == "DURABLE" ? svc_bus_queue_mode_t.SERVICE_BUS_DURABLE : svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);

                    var sqlMapPath = AppDomain.CurrentDomain.BaseDirectory + ibatisConfig;
                    IRepositoryFactory factory = new RepositoryFactory(sqlMapPath, ibatisConnString);

                    //var remoteMicroservice = new RemoteMicroservice(ps, Builder.newService(remoteServiceName, svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE), logger, producerName);
                    //scope.add(remoteMicroservice);

                    var diInputDto = new DIInputDto
                    {
                        MyParameter = myParameter,
                        RepositoryFactory = factory,
                        //RemoteMicroservice = remoteMicroservice
                    };
                    var producerName = string.Format("TemplateDispatcher-{0}-PID:{1}", Environment.MachineName, Process.GetCurrentProcess().Id);
                    var dispatcher = scope.newDispatcher(producerName, svc, ps, typeof(TemplateDispatchInterface));
                    dispatcher.Logger = logger;

                    var chain = new DestinationRequestProcessingStrategy(new PingRequestStrategy(new FileLogRequestStrategy(logger)));
                    dispatcher.RequestProcessingStrategy = chain;
                    dispatcher.ResponseProcessingStrategy = new FileLogResponseStrategy(logger);
                    dispatcher.Start(threadCount, dispatchMode == "MULTI" ? DispatchMode.MultiInstanceDispatchInterface : DispatchMode.SingletonDispatchInterface, diInputDto);

                    var inrprtr = new CmdInterpreter<TemplateDispatchInterface>(diInputDto, null);

                    new ConsoleMicroservice().Start("Template", inrprtr, logger);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    if (logger != null)
                        logger.LogException(e, "Service exiting due to exception");

                    Environment.ExitCode = -1;
                }
            }
        }
    }
}

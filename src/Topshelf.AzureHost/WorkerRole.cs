using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using Topshelf.Logging;
using Topshelf.NancyDashboard;

namespace Topshelf.AzureHost
{
    public class WorkerRole : RoleEntryPoint
    {
        static readonly ILog _log = Logger.Get(typeof(WorkerRole));

        private readonly ManualResetEvent waitForStop = new ManualResetEvent(false);
        private AzureRunHost _host;

        public override void Run()
        {
            Trace.WriteLine("Topshelf.AzureHost entry point called", "Information");

            _host.Run();

            waitForStop.WaitOne();
        }

        public override void OnStop()
        {
            _host.Stop();

            base.OnStop();
        }

        private void ConfigureHost()
        {
            BootstrapLogger();

            _host = (AzureRunHost)HostFactory.New(x =>
            {
                x.BeforeStartingServices(() => Trace.WriteLine("[Topshelf] Preparing to start host services"));

                x.AfterStartingServices(() => Trace.WriteLine("[Topshelf] All services have been started"));

                x.UseBuilder(description => new AzureHostBuilder(description));

                x.SetServiceName(ShelfHost.DefaultServiceName);
                x.SetDisplayName(ShelfHost.DefaultServiceName);
                x.SetDescription("Topshelf Azure Host");

                x.EnableDashboardWebServices();

                x.Service<ShelfHost>(y =>
                {
                    y.SetServiceName(ShelfHost.DefaultServiceName);
                    y.ConstructUsing((name, coordinator) => new ShelfHost(coordinator));
                    y.WhenStarted(host => host.Start());
                    y.WhenStopped(host => host.Stop());
                });

                x.AfterStoppingServices(() =>
                                            {
                                                Trace.WriteLine("[Topshelf] All services have been stopped");
                                                waitForStop.Set();
                                            });
            });
        }

        static void BootstrapLogger()
        {
            string configurationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");

            Log4NetLogger.Use(configurationFilePath);

            _log.DebugFormat("Logging configuration loaded: {0}", configurationFilePath);
        }

        public override bool OnStart()
        {
            ConfigureHost();

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}

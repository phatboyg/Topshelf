namespace Topshelf.NancyDashboard
{
    using System;
    using System.Configuration;
    using System.Net;
    using Nancy.Hosting.Self;
    using Topshelf;
    using Logging;
    using Model;


    public class TopshelfDashboard
    {
        readonly ILog _log = Logger.Get("Topshelf.WebControl.WebControl");

        readonly IServiceChannel _serviceCoordinator;
        NancyHost _nancyHost;

        public TopshelfDashboard(ServiceDescription description, IServiceChannel serviceCoordinator)
        {
            _serviceCoordinator = serviceCoordinator;
            TinyIoC.TinyIoCContainer.Current.Register<IServiceChannel>(_serviceCoordinator);
        }

        public void Start()
        {
            var configuration = new DashboardConfiguration
                {
                    ServerUri = new Uri("http://localhost:8085"),
                    EnablePackageUploads = false,
                    ServicesLocation = "Services",
                    PackageStore = "Packages"
                };

            if (StartHttpListener(configuration))
            {
                ConfigurePackageUploads(configuration);

                TinyIoC.TinyIoCContainer.Current.Register(configuration);
            }
        }

        bool StartHttpListener(DashboardConfiguration configuration)
        {
            string listenUrl = ConfigurationManager.AppSettings["DashboardUri"];
            if (! string.IsNullOrEmpty(listenUrl))
            {
                try
                {
                    configuration.ServerUri = new Uri(listenUrl);
                }
                catch (Exception ex)
                {
                    _log.Error("Incorrect DashboardUri format. Using the default setting.", ex);
                }
            }

            _log.InfoFormat("Loading dashboard at Uri: {0}", configuration.ServerUri);

            try
            {
                _nancyHost = new NancyHost(configuration.ServerUri);
                _nancyHost.Start();
            }
            catch (HttpListenerException ex)
            {
                _nancyHost = null;
                if (ex.Message == "Access is denied")
                {
                    string enableCommand = string.Format(@"netsh http add urlacl url=http://+:{0}{1} user=your_domain\service_user", configuration.ServerUri.Port, configuration.ServerUri.AbsolutePath);
                    _log.Error("Failed to initialize HTTP listener.\r\nThe process is not allowed listening on the specified port without permission.\r\nConsider running the following command: " + enableCommand, ex);
                }
                else
                    _log.Error("Failed to initialize HTTP listener.", ex);
            }
            catch (Exception ex)
            {
                _nancyHost = null;
                _log.Error("Failed to initialize HTTP listener.", ex);
            }

            return _nancyHost != null;
        }

        static void ConfigurePackageUploads(DashboardConfiguration configuration)
        {
            // ServicesLocation
            string servicesLocation = ConfigurationManager.AppSettings["ServicesLocation"];
            if (!string.IsNullOrEmpty(servicesLocation))
                configuration.ServicesLocation = servicesLocation;

            // DashboardUploadsEnable
            string enablePackageUploadsValue = ConfigurationManager.AppSettings["DashboardUploadsEnable"];
            bool enablePackageUploads = false;
            if (bool.TryParse(enablePackageUploadsValue, out enablePackageUploads))
                configuration.EnablePackageUploads = enablePackageUploads;

            // DashboardUploadsPackageFolder
            string packageStore = ConfigurationManager.AppSettings["DashboardUploadsPackageFolder"];
            if (! string.IsNullOrEmpty(packageStore))
                configuration.PackageStore = packageStore;
        }

        public void Stop()
        {
            if (_nancyHost != null)
                _nancyHost.Stop();
            _nancyHost = null;
        }
    }
}
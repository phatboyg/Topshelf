namespace Topshelf.NancyDashboard
{
    using System;
    using System.Configuration;
    using System.Net;
    using Nancy.Hosting.Self;
    using Topshelf;
    using Topshelf.Logging;
    using Topshelf.Model;


    public class TopshelfDashboard
    {
        readonly ILog _log = Logger.Get("Topshelf.WebControl.WebControl");

        readonly ServiceDescription _description;
        readonly IServiceChannel _serviceCoordinator;
        NancyHost _nancyHost;

        public Uri ServerUri { get; set; }

        public TopshelfDashboard(ServiceDescription description, IServiceChannel serviceCoordinator)
        {
            _description = description;
            _serviceCoordinator = serviceCoordinator;
            TinyIoC.TinyIoCContainer.Current.Register<IServiceChannel>(_serviceCoordinator);
        }

        public void Start()
        {
            ServerUri = new Uri("http://localhost:8085");
            
            string listenUrl = ConfigurationManager.AppSettings["DashboardUri"];
            if (! string.IsNullOrEmpty(listenUrl))
            {
                try
                {
                    ServerUri = new Uri(listenUrl);
                }
                catch (Exception ex)
                {
                    _log.Error("Incorrect DashboardUri format. Using the default setting.", ex);
                }
            }
            
            _log.InfoFormat("Loading dashboard at Uri: {0}", ServerUri);

            try
            {
                _nancyHost = new NancyHost(ServerUri);
                _nancyHost.Start();
            }
            catch (HttpListenerException ex)
            {
                _nancyHost = null;
                if (ex.Message == "Access is denied")
                {
                    string enableCommand = string.Format(@"netsh http add urlacl url=http://+:{0}{1} user=your_domain\service_user", ServerUri.Port, ServerUri.AbsolutePath);
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
        }

        public void Stop()
        {
            if (_nancyHost != null)
                _nancyHost.Stop();
            _nancyHost = null;
        }
    }
}
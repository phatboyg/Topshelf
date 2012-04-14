using System;
using System.IO;
using System.Threading;
using Topshelf.Internal;
using Topshelf.Logging;
using Topshelf.Model;
using Topshelf.OS;

namespace Topshelf.AzureHost
{
    public class AzureRunHost : Host, IDisposable
    {
        readonly ServiceDescription _description;
		readonly ILog _log = Logger.Get("Topshelf.Hosts.ConsoleRunHost");
		IServiceCoordinator _coordinator;
		ManualResetEvent _exit;
        volatile bool _hasCancelled;
        Os _osCommands;

        public AzureRunHost([NotNull] ServiceDescription description, [NotNull] IServiceCoordinator coordinator, [NotNull]Os osCommands)
		{
			if (description == null)
				throw new ArgumentNullException("description");
			if (coordinator == null)
				throw new ArgumentNullException("coordinator");
            if(osCommands ==null)
                throw new ArgumentNullException("osCommands");

			_description = description;
			_coordinator = coordinator;
		    _osCommands = osCommands;
		}

		public void Run()
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			_osCommands.CheckToSeeIfServiceRunning(_description);

			try
			{
				_log.Debug("Starting up as Azure worker role");

				_exit = new ManualResetEvent(false);

				_coordinator.Start(); //user code starts

				_log.InfoFormat("[Topshelf] Running.");

				_exit.WaitOne();
			}
			catch (Exception ex)
			{
				_log.Error("An exception occurred", ex);
			}
			finally
			{
				ShutdownCoordinator();
				_exit.Close();
			}
		}

		void ShutdownCoordinator()
		{
			try
			{
				_log.Info("[Topshelf] Stopping");

				_coordinator.Stop();
			}
			catch (Exception ex)
			{
				_log.Error("The service did not shut down gracefully", ex);
			}
			finally
			{
				_coordinator.Dispose();
				_coordinator = null;

				_log.Info("[Topshelf] Stopped");
			}
		}

		public void Stop()
		{
			_log.Info("Received Stop signal.");
			_exit.Set();

			_hasCancelled = true;
		}


		bool _disposed;

        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				(_exit as IDisposable).Dispose();
			}

			_disposed = true;
		}
    }
}
namespace Topshelf.NancyDashboard
{
    using System;

    /// <summary>
    /// Configuration settings block for the Dashboard behavior
    /// </summary>
    public class DashboardConfiguration
    {
        /// <summary>
        /// Defines location of the Unpacked services (shelves)
        /// </summary>
        public string ServicesLocation { get; set; }

        /// <summary>
        /// Defines a listenning Uri for the dashboard
        /// </summary>
        public Uri ServerUri { get; set; }

        /// <summary>
        /// Controlls if a UI will allow package uploads
        /// </summary>
        public bool EnablePackageUploads { get; set; }

        /// <summary>
        /// Defines a temporary location of the uploaded packages
        /// </summary>
        public string PackageStore { get; set; }
    }
}
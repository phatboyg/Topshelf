namespace Topshelf.Options
{
    using System;
    using Builders;
    using HostConfigurators;
    using Internal;


    public class ShelfOption :
        Option
    {
        public ShelfOption(string uri, string pipe, string bootstrapper)
        {
            Uri = new Uri(uri);
            Pipe = pipe;
            Bootstrapper = bootstrapper;
        }

        public Uri Uri { get; set; }
        public string Pipe { get; set; }
        public string Bootstrapper { get; set; }


        public void ApplyTo([NotNull] HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            
            configurator.UseBuilder(description => new ProcessShelfBuilder(description, Uri, Pipe, Bootstrapper));
        }
    }
}
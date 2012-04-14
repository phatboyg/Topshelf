using Topshelf.Builders;

namespace Topshelf.AzureHost
{
    public class AzureHostBuilder : RunBuilder
    {
        private readonly ServiceDescription _description;

        public AzureHostBuilder(ServiceDescription description) : base(description)
        {
            _description = description;
        }

        public override Host CreateHost(Model.IServiceCoordinator coordinator, OS.Os osCommands)
        {
            return new AzureRunHost(_description, coordinator, osCommands);
        }
    }
}
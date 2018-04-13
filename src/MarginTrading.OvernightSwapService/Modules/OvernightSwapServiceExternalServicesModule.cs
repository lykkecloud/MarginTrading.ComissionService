using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.SettingsReader;
using MarginTrading.OvernightSwapService.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.OvernightSwapService.Modules
{
    internal class OvernightSwapServiceExternalServicesModule : Module
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly IReloadingManager<AppSettings> _settings;

        public OvernightSwapServiceExternalServicesModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // todo register external services here
            builder.Populate(_services);
        }
    }
}
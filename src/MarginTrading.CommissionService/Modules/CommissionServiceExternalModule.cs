using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Service.ClientAccount.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using MarginTrading.CommissionService.Core.Settings;

namespace Lykke.MarginTrading.CommissionService.Modules
{
    internal class CommissionServiceExternalModule : Module
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly IReloadingManager<AppSettings> _settings;

        public CommissionServiceExternalModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterLykkeServiceClient(_settings.CurrentValue.CommissionService.Services.
                ClientAccount.Url);
            
            builder.Populate(_services);
        }
    }
}
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.HttpClientGenerator;
using Lykke.SettingsReader;
using Lykke.Snow.Common.Startup;
using MarginTrading.Backend.Contracts;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Infrastructure;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.TradingHistory.Client;
using Microsoft.Extensions.DependencyInjection;
using IAccountsApi = MarginTrading.AccountsManagement.Contracts.IAccountsApi;

namespace MarginTrading.CommissionService.Modules
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
            RegisterClientWithName<IPositionsApi>(builder,
                "MT Trading Core",
                _settings.CurrentValue.CommissionService.Services.Backend.Url);
            
            RegisterClientWithName<IOrderEventsApi>(builder,
                "MT Trading History",
                _settings.CurrentValue.CommissionService.Services.TradingHistory.Url);

            RegisterClientWithName<IAccountsApi>(builder,
                "MT Accounts Management",
                _settings.CurrentValue.CommissionService.Services.AccountManagement.Url);
            
            RegisterClientWithName<IAssetsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            
            RegisterClientWithName<IAssetPairsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            
            RegisterClientWithName<ITradingConditionsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            
            RegisterClientWithName<ITradingInstrumentsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            
            builder.Populate(_services);
        }

        private void RegisterClientWithName<TApi>(ContainerBuilder builder, string name, string uri)
            where TApi : class
        {
            builder.RegisterClient<TApi>(uri,
                config => config.WithServiceName<LykkeErrorResponse>($"{name} [{uri}]"));
        }
    }
}
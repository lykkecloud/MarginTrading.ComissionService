using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.HttpClientGenerator;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts;
using MarginTrading.CommissionService.Core.Settings;
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
            builder.RegisterClient<IPositionsApi>(_settings.CurrentValue.CommissionService.Services.Backend.Url, 
                config => config.WithApiKey(_settings.CurrentValue.CommissionService.Services.Backend.ApiKey));

            builder.RegisterClient<IOrdersHistoryApi>(
                _settings.CurrentValue.CommissionService.Services.TradingHistory.Url);
                //, config => config.WithApiKey(_settings.CurrentValue.CommissionService.Services.TradingHistory.ApiKey));
            
            builder.RegisterClient<IAccountsApi>(_settings.CurrentValue.CommissionService.Services.AccountManagement.Url);
            
            builder.RegisterClient<IAssetsApi>(_settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            builder.RegisterClient<IAssetPairsApi>(_settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            builder.RegisterClient<ITradingConditionsApi>(_settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            builder.RegisterClient<ITradingInstrumentsApi>(_settings.CurrentValue.CommissionService.Services.SettingsService.Url);
            
            builder.Populate(_services);
        }
    }
}
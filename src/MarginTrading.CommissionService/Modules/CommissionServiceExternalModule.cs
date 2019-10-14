// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
                _settings.CurrentValue.CommissionService.Services.Backend);
            
            RegisterClientWithName<IPricesApi>(builder,
                "MT Trading Core",
                _settings.CurrentValue.CommissionService.Services.Backend);
            
            RegisterClientWithName<IOrderEventsApi>(builder,
                "MT Trading History",
                _settings.CurrentValue.CommissionService.Services.TradingHistory);

            RegisterClientWithName<IAccountsApi>(builder,
                "MT Accounts Management",
                _settings.CurrentValue.CommissionService.Services.AccountManagement);
            
            RegisterClientWithName<IAssetsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService);
            
            RegisterClientWithName<IAssetPairsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService);
            
            RegisterClientWithName<ITradingConditionsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService);
            
            RegisterClientWithName<ITradingInstrumentsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService);
            
            RegisterClientWithName<IScheduleSettingsApi>(builder,
                "MT Settings",
                _settings.CurrentValue.CommissionService.Services.SettingsService);
            
            builder.Populate(_services);
        }

        private static void RegisterClientWithName<TApi>(ContainerBuilder builder, string name, 
            ServiceSettings serviceSettings)
            where TApi : class
        {
            builder.RegisterClient<TApi>(serviceSettings.Url,
                config =>
                {
                    var httpClientGeneratorBuilder = config.WithServiceName<LykkeErrorResponse>($"{name} [{serviceSettings.Url}]");

                    if (!string.IsNullOrEmpty(serviceSettings.ApiKey))
                    {
                        httpClientGeneratorBuilder = httpClientGeneratorBuilder.WithApiKey(serviceSettings.ApiKey);
                    }
                    
                    return httpClientGeneratorBuilder;
                });
        }
    }
}
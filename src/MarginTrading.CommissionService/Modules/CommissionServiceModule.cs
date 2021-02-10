// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Chaos;
using Lykke.SettingsReader;
using Lykke.Snow.Mdm.Contracts.Api;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.Services.Caches;
using MarginTrading.CommissionService.Services.Handlers;
using MarginTrading.CommissionService.Services.OrderDetailsFeature;
using MarginTrading.CommissionService.SqlRepositories.Repositories;
using Microsoft.Extensions.Internal;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Modules
{
    internal class CommissionServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public CommissionServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings.Nested(s => s.CommissionService)).SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue.CommissionService).SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue.CommissionService.RequestLoggerSettings).SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue.CommissionService.CostsAndChargesDefaults).SingleInstance();
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterType<SystemClock>().As<ISystemClock>().SingleInstance();
            
            builder.RegisterType<RabbitMqService>().As<IRabbitMqService>().SingleInstance();

            builder.RegisterType<CqrsMessageSender>()
                .As<ICqrsMessageSender>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
            
            builder.RegisterType<ThreadSwitcherToNewTask>()
                .As<IThreadSwitcher>()
                .SingleInstance();
            
            builder.RegisterType<SystemClock>()
                .As<ISystemClock>()
                .SingleInstance();

            builder.RegisterInstance(new ConsoleLWriter(Console.WriteLine))
                .As<IConsole>()
                .SingleInstance();
            
            builder.RegisterChaosKitty(_settings.CurrentValue.CommissionService.ChaosKitty);

            RegisterRepositories(builder);
            RegisterServices(builder);
            RegisterRedis(builder);
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<CfdCalculatorService>()
                .As<ICfdCalculatorService>()
                .SingleInstance();
            
            builder.RegisterType<CommissionCalcService>()
                .As<ICommissionCalcService>()
                .SingleInstance();

            builder.RegisterType<ExecutedOrdersHandlingService>()
                .As<IExecutedOrdersHandlingService>()
                .SingleInstance();
            
            builder.RegisterType<ConvertService>()
                .As<IConvertService>()
                .SingleInstance();
           
            builder.RegisterType<PositionReceiveService>()
                .As<IPositionReceiveService>()
                .SingleInstance();
            
            builder.RegisterType<OvernightSwapService>()
                .As<IOvernightSwapService>()
                .SingleInstance();
            
            builder.RegisterType<DailyPnlService>()
                .As<IDailyPnlService>()
                .SingleInstance();

            builder.RegisterType<QuoteCacheService>()
                .As<IQuoteCacheService>()
                .SingleInstance()
                .OnActivated(args => args.Instance.Start());

            builder.RegisterType<FxRateCacheService>()
                .As<IFxRateCacheService>()
                .SingleInstance()
                .OnActivated(args => args.Instance.Start());

            builder.RegisterType<InterestRatesCacheService>()
                .As<IInterestRatesCacheService>()
                .SingleInstance()
                .OnActivated(args => args.Instance.InitCache());
            
            builder.RegisterType<AssetsCache>()
                .As<IAssetsCache>()
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterType<AssetPairsCache>()
                .As<IAssetPairsCache>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<OvernightSwapListener>()
                .As<IOvernightSwapListener>()
                .SingleInstance();

            builder.RegisterType<DailyPnlListener>()
                .As<IDailyPnlListener>()
                .SingleInstance();

            builder.RegisterType<RateSettingsService>()
                .As<IRateSettingsService>()
                .As<IRateSettingsCache>()
                .SingleInstance();

            builder.RegisterType<AccountRedisCache>()
                .As<IAccountRedisCache>()
                .SingleInstance();

            builder.RegisterType<CostsAndChargesGenerationService>()
                .As<ICostsAndChargesGenerationService>()
                .SingleInstance();

            builder.RegisterType<TradingInstrumentsCache>()
                .As<ITradingInstrumentsCache>()
                .SingleInstance();
            
            builder.RegisterType<TradingDaysInfoProvider>()
                .As<ITradingDaysInfoProvider>()
                .SingleInstance();

            builder.Register<IReportGenService>(ctx =>
                new ReportGenService(
                    ctx.Resolve<IAssetsCache>(), 
                    "./Fonts/",
                    _settings.CurrentValue.CommissionService.ReportSettings.TimeZonePartOfTheName))
                .SingleInstance();

            builder.Register<IFontProvider>(ctx => new FontProvider("./Fonts/"))
                .SingleInstance();

            builder.RegisterType<ClientProfileCache>()
                .As<IStartable>()
                .As<IClientProfileCache>()
                .SingleInstance();
            
            builder.RegisterType<ClientProfileSettingsCache>()
                .As<IStartable>()
                .As<IClientProfileSettingsCache>()
                .SingleInstance();
            
            builder.RegisterType<CacheUpdater>()
                .As<IStartable>()
                .As<ICacheUpdater>()
                .SingleInstance();
            
            builder.RegisterType<UnderlyingChangedHandler>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<BrokerSettingsChangedHandler>()
                .AsSelf()
                .SingleInstance();

            builder.Register<IBrokerSettingsService>(ctx =>
                    new BrokerSettingsService(
                        ctx.Resolve<IBrokerSettingsApi>(),
                        _settings.CurrentValue.CommissionService.BrokerId))
                .SingleInstance();

            builder.RegisterType<ProductCostCalculationService>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<OrderDetailsCalculationService>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<OrderDetailsSpanishLocalizationService>()
                .AsImplementedInterfaces()
                .SingleInstance();
            
            builder.RegisterType<OrderDetailsDataSourceBuilder>()
                .AsImplementedInterfaces()
                .SingleInstance();
            
            builder.RegisterType<OrderDetailsPdfGenerator>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            if (_settings.CurrentValue.CommissionService.Db.StorageMode != StorageMode.SqlServer)
            {
                throw new InvalidOperationException("Storage mode other than SqlServer is not supported");
            }

            builder.RegisterType<OvernightSwapHistoryRepository>()
                .As<IOvernightSwapHistoryRepository>()
                .SingleInstance();

            builder.RegisterType<DailyPnlHistoryRepository>()
                .As<IDailyPnlHistoryRepository>()
                .SingleInstance();

            builder.RegisterType<InterestRatesRepository>()
                .As<IInterestRatesRepository>()
                .SingleInstance();

            builder.RegisterType<OperationExecutionInfoRepository>()
                .As<IOperationExecutionInfoRepository>()
                .SingleInstance();

            builder.RegisterType<CostsAndChargesRepository>()
                .As<ICostsAndChargesRepository>()
                .SingleInstance();

            builder.RegisterType<TradingEngineSnapshotRepository>()
                .As<ITradingEngineSnapshotRepository>()
                .SingleInstance();

            builder.RegisterType<SharedCostsAndChargesRepository>()
                .As<ISharedCostsAndChargesRepository>()
                .SingleInstance();

            builder.Register(provider =>
                new CommissionHistoryRepository(_settings.CurrentValue.CommissionService.Db.StateConnString))
                .AsImplementedInterfaces();
            
        }

        private void RegisterRedis(ContainerBuilder builder)
        {
            builder.Register(c => ConnectionMultiplexer.Connect(
                    _settings.CurrentValue.CommissionService.RedisSettings.Configuration))
                .As<IConnectionMultiplexer>()
                .SingleInstance();

            builder.Register(c => c.Resolve<IConnectionMultiplexer>().GetDatabase())
                .As<IDatabase>();
        }
    }
}
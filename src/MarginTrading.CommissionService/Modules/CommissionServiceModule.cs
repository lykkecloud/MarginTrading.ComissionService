// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Chaos;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.AzureRepositories;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.Services.Caches;
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
            builder.RegisterInstance(_settings.CurrentValue.CommissionService.DefaultRateSettings).SingleInstance();
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterType<SystemClock>().As<ISystemClock>().SingleInstance();
            
            builder.RegisterType<RabbitMqService>().As<IRabbitMqService>().SingleInstance();
            
            builder.RegisterType<EventSender>().As<IEventSender>()
                .WithParameters(new[]
                {
                    new TypedParameter(typeof(RabbitMqSettings), _settings.CurrentValue.CommissionService.RabbitMq), 
                })
                .SingleInstance();

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
            
            builder.RegisterType<SettingsManager>()
                .AsSelf()
                .As<IStartable>()
                .As<ISettingsManager>()
                .SingleInstance();

            builder.RegisterType<OvernightSwapListener>()
                .As<IOvernightSwapListener>()
                .SingleInstance();

            builder.RegisterType<DailyPnlListener>()
                .As<IDailyPnlListener>()
                .SingleInstance();

            builder.RegisterType<RateSettingsService>()
                .As<IRateSettingsService>()
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
                new ReportGenService(ctx.Resolve<IAssetsCache>(), "./Fonts/")).SingleInstance();
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            if (_settings.CurrentValue.CommissionService.Db.StorageMode != StorageMode.SqlServer)
            {
                throw new InvalidOperationException("Storage mode other than SqlServer is not supported");
            }

            builder.Register<IMarginTradingBlobRepository>(ctx =>
                    new SqlBlobRepository(_settings.CurrentValue.CommissionService.Db.StateConnString))
                .SingleInstance();

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
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
                .SingleInstance();
            
            builder.RegisterType<AssetPairsCache>()
                .As<IAssetPairsCache>()
                .As<IAssetPairsInitializableCache>()
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterType<AssetPairsManager>()
                .AsSelf()
                .As<IStartable>()
                .As<IAssetPairsManager>()
                .SingleInstance();

            builder.RegisterType<FxRateCacheService>()
                .As<IFxRateCacheService>()
                .SingleInstance()
                .OnActivated(args => args.Instance.Start());
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            if (_settings.CurrentValue.CommissionService.Db.StorageMode == StorageMode.Azure)
            {
                builder.Register<IMarginTradingBlobRepository>(ctx =>
                    AzureRepoFactories.MarginTrading.CreateBlobRepository(_settings.Nested(s =>
                        s.CommissionService.Db.StateConnString))).SingleInstance();

                builder.Register<IOvernightSwapHistoryRepository>(ctx =>
                        AzureRepoFactories.MarginTrading.CreateOvernightSwapHistoryRepository(
                            _settings.Nested(s => s.CommissionService.Db.StateConnString), _log))
                    .SingleInstance();
                
                builder.Register<IOperationExecutionInfoRepository>(ctx =>
                        AzureRepoFactories.MarginTrading.CreateOperationExecutionInfoRepository(
                            _settings.Nested(s => s.CommissionService.Db.StateConnString), _log, ctx.Resolve<ISystemClock>()))
                    .SingleInstance();
            } 
            else if (_settings.CurrentValue.CommissionService.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.Register<IMarginTradingBlobRepository>(ctx =>
                        new SqlBlobRepository(_settings.CurrentValue.CommissionService.Db.StateConnString))
                    .SingleInstance();
                
                builder.RegisterType<OvernightSwapHistoryRepository>()
                    .As<IOvernightSwapHistoryRepository>()
                    .SingleInstance();

                builder.RegisterType<OperationExecutionInfoRepository>()
                    .As<IOperationExecutionInfoRepository>()
                    .SingleInstance();
            }
        }
    }
}
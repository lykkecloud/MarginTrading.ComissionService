using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.AzureRepositories;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.Services.Caches;
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
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterType<SystemClock>().As<ISystemClock>().SingleInstance();

            builder.RegisterType<OvernightSwapCache>().As<IOvernightSwapCache>().SingleInstance();
            
            builder.RegisterType<ThreadSwitcherToNewTask>()
                .As<IThreadSwitcher>()
                .SingleInstance();
            
            builder.RegisterType<SystemClock>()
                .As<ISystemClock>()
                .SingleInstance();

            RegisterRepositories(builder);
            RegisterServices(builder);
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<CfdCalculatorService>()
                .As<ICfdCalculatorService>()
                .SingleInstance();
            
            builder.RegisterType<global::MarginTrading.CommissionService.Services.CommissionCalcService>()
                .As<ICommissionCalcService>()
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

            builder.RegisterType<QuoteCacheService>()
                .As<IQuoteCacheService>()
                .SingleInstance();
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
                            _settings.Nested(s => s.CommissionService.Db.MarginTradingConnString), _log))
                    .SingleInstance();
            } 
            else if (_settings.CurrentValue.CommissionService.Db.StorageMode == StorageMode.SqlServer)
            {
                
            }
        }
    }
}
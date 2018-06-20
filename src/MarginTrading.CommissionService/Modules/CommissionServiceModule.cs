using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.AzureRepositories;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.Services.Caches;
using Microsoft.Extensions.Internal;

namespace Lykke.MarginTrading.CommissionService.Modules
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
            RegisterDefaultImplementations(builder);

            builder.RegisterInstance(_settings.Nested(s => s.CommissionService)).SingleInstance();
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterType<SystemClock>().As<ISystemClock>().SingleInstance();

            builder.RegisterType<AssetPairsCache>().As<IAssetPairsCache>().SingleInstance();
            builder.RegisterType<AssetsCache>().As<IAssetsCache>().SingleInstance();
            builder.RegisterType<OvernightSwapCache>().As<IOvernightSwapCache>().SingleInstance();

            builder.RegisterType<AccountAssetsCacheService>()
                .AsSelf().SingleInstance();
            
            builder.RegisterType<ThreadSwitcherToNewTask>()
                .As<IThreadSwitcher>()
                .SingleInstance();
            
            builder.RegisterType<FakeAccountManager>()
                .As<IAccountManager>()
                .SingleInstance();
            
            builder.RegisterType<FakeEmailService>()
                .As<IEmailService>()
                .SingleInstance();

            RegisterStorages(builder);
        }

        /// <summary>
        /// Scans for types in the current assembly and registers types which: <br/>
        /// - are named like 'SmthService' <br/>
        /// - implement an non-generic interface named like 'ISmthService' in the same assembly <br/>
        /// - are the only implementations of the 'ISmthService' interface <br/>
        /// - are not generic <br/><br/>
        /// Types like SmthRepository are also supported.
        /// Also registers startup for implementations of <see cref="IStartable"/>.
        /// </summary>
        private void RegisterDefaultImplementations(ContainerBuilder builder)
        {
            var assembly = GetType().Assembly;
            var implementations = assembly.GetTypes()
                .Where(t => !t.IsInterface && !t.IsGenericType && (t.Name.EndsWith("Service") ))//|| t.Name.EndsWith("Repository")))
                .SelectMany(t =>
                    t.GetInterfaces()
                        .Where(i => i.Name.StartsWith('I') && i.Assembly == assembly)
                        .Select(i => (Implementation: t, Interface: i)))
                .GroupBy(t => t.Interface)
                .Where(gr => gr.Count() == 1)
                .SelectMany(gr => gr);

            foreach (var t in implementations)
            {
                var registrationBuilder = builder.RegisterType(t.Implementation).As(t.Interface).SingleInstance();
                if (typeof(IStartable).IsAssignableFrom(t.Implementation))
                    registrationBuilder.OnActivated(args => ((IStartable) args.Instance).Start()).AutoActivate();
            }
        }

        private void RegisterStorages(ContainerBuilder builder)
        {
            builder.Register<IMarginTradingBlobRepository>(ctx =>
                AzureRepoFactories.MarginTrading.CreateBlobRepository(_settings.Nested(s =>
                    s.CommissionService.Db.StateConnString))).SingleInstance();

            builder.Register<IOvernightSwapStateRepository>(ctx =>
                    AzureRepoFactories.MarginTrading.CreateOvernightSwapStateRepository(
                        _settings.Nested(s => s.CommissionService.Db.MarginTradingConnString), _log))
                .SingleInstance();

            builder.Register<IOvernightSwapHistoryRepository>(ctx =>
                    AzureRepoFactories.MarginTrading.CreateOvernightSwapHistoryRepository(
                        _settings.Nested(s => s.CommissionService.Db.MarginTradingConnString), _log))
                .SingleInstance();
            
            builder.Register<IAccountAssetPairsRepository>(ctx =>
                    AzureRepoFactories.MarginTrading.CreateAccountAssetsRepository(
                        _settings.Nested(s => s.CommissionService.Db.MarginTradingConnString), _log))
                .SingleInstance();
        }
    }
}
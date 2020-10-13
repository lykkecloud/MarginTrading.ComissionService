// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using Common.Log;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Extensions;
using MarginTrading.CommissionService.Services.Handlers;
using MarginTrading.CommissionService.Subscribers;

namespace MarginTrading.CommissionService.Modules
{
    public class RabbitMqModule : Module
    {
        private CommissionServiceSettings _settings;
        private ILog _log;

        public RabbitMqModule(CommissionServiceSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => new UnderlyingChangedSubscriber(x.Resolve<UnderlyingChangedHandler>(),
                    _settings.UnderlyingChangedRabbitSubscriptionSettings.AppendToQueueName($"{_settings.InstanceId}:{_settings.BrokerId}"), _log))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
            
            builder.Register(x => new BrokerSettingsSubscriber(x.Resolve<BrokerSettingsChangedHandler>(),
                    _settings.BrokerSettingsChangedRabbitSubscriptionSettings.AppendToQueueName($"{_settings.InstanceId}:{_settings.BrokerId}"), _log))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
        }
    }
}
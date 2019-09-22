// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.Cqrs.Configuration.Routing;
using Lykke.Cqrs.Configuration.Saga;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;
using MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Events;
using MarginTrading.CommissionService.Core.Workflow.OvernightSwap.Events;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.Workflow;
using MarginTrading.CommissionService.Workflow.ChargeCommission;
using MarginTrading.CommissionService.Workflow.DailyPnl;
using MarginTrading.CommissionService.Workflow.OnBehalf;
using MarginTrading.CommissionService.Workflow.OvernightSwap;

namespace MarginTrading.CommissionService.Modules
{
    internal class CqrsModule : Module
    {
        private const string DefaultRoute = "self";
        private const string DefaultPipeline = "commands";
        private readonly CqrsSettings _settings;
        private readonly ILog _log;
        private readonly long _defaultRetryDelayMs;
        private readonly CqrsContextNamesSettings _contextNames;

        public CqrsModule(CqrsSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
            _defaultRetryDelayMs = (long) _settings.RetryDelay.TotalMilliseconds;
            _contextNames = _settings.ContextNames;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();
            builder.RegisterInstance(_contextNames).AsSelf().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.ConnectionString
            };

            var messagingEngine = new MessagingEngine(_log, new TransportResolver(
                new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }), new RabbitMqTransportFactory());
            
            // Sagas & command handlers
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => t.Name.EndsWith("Saga") || t.Name.EndsWith("CommandsHandler") || t.Name.EndsWith("Projection"))
                .AsSelf();

            builder.Register(ctx => CreateEngine(ctx, messagingEngine))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var rabbitMqConventionEndpointResolver = new RabbitMqConventionEndpointResolver(
                "RabbitMq",
                "messagepack",
                environment: _settings.EnvironmentName);
            return new CqrsEngine(
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(rabbitMqConventionEndpointResolver),
                RegisterDefaultRouting(),
                RegisterChargeCommissionSaga(),
                RegisterAccountListenerSaga(),
                RegisterContext());
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_contextNames.CommissionService)
                .FailedCommandRetryDelay(_defaultRetryDelayMs)
                .ProcessingOptions(DefaultRoute)
                .MultiThreaded(_settings.CommandsHandlersThreadCount)
                .QueueCapacity(_settings.CommandsHandlersQueueCapacity);
            RegisterOrderExecCommissionCommandHandler(contextRegistration);
            RegisterOnBehalfCommandsHandler(contextRegistration);
            RegisterOvernightSwapCommandHandler(contextRegistration);
            RegisterDailyPnlCommandsHandler(contextRegistration);
            return contextRegistration;
        }

        private PublishingCommandsDescriptor<IDefaultRoutingRegistration> RegisterDefaultRouting()
        {
            return Register.DefaultRouting
                .PublishingCommands(
                    typeof(HandleOrderExecInternalCommand),
                    typeof(HandleOnBehalfInternalCommand),
                    typeof(StartOvernightSwapsProcessCommand),
                    typeof(StartDailyPnlProcessCommand))
                .To(_contextNames.CommissionService)
                .With(DefaultPipeline);
        }
        
        private IRegistration RegisterChargeCommissionSaga()
        {
            var sagaRegistration = RegisterSaga<ChargeCommissionSaga>();
            
            sagaRegistration
                .ListeningEvents(
                    typeof(OrderExecCommissionCalculatedInternalEvent),
                    typeof(OnBehalfCalculatedInternalEvent),
                    typeof(OvernightSwapCalculatedInternalEvent),
                    typeof(DailyPnlCalculatedInternalEvent),
                    
                    typeof(DailyPnlsCalculatedEvent),
                    typeof(OvernightSwapsCalculatedEvent),
                    
                    typeof(OvernightSwapsStartFailedEvent),
                    typeof(OvernightSwapsChargedEvent),
                    typeof(DailyPnlsStartFailedEvent),
                    typeof(DailyPnlsChargedEvent)
                )
                .From(_contextNames.CommissionService)
                .On(DefaultRoute)
                .PublishingCommands(
                    typeof(ChangeBalanceCommand)
                )
                .To(_contextNames.AccountsManagement)
                .With(DefaultPipeline);
            
            sagaRegistration
                .PublishingCommands(
                    typeof(ChargeSwapsTimeoutInternalCommand),
                    typeof(ChargeDailyPnlTimeoutInternalCommand)
                )
                .To(_contextNames.CommissionService)
                .With(DefaultPipeline);

            return sagaRegistration;
        }

        private static void RegisterOrderExecCommissionCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(HandleOrderExecInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<OrderExecCommissionCommandsHandler>()
                .PublishingEvents(
                    typeof(OrderExecCommissionCalculatedInternalEvent))
                .With(DefaultPipeline);
        }
        
        private static void RegisterOnBehalfCommandsHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(HandleOnBehalfInternalCommand)
                )
                .On(DefaultRoute)
                .WithCommandsHandler<OnBehalfCommandsHandler>()
                .PublishingEvents(
                    typeof(OnBehalfCalculatedInternalEvent))
                .With(DefaultPipeline);
        }

        private static void RegisterOvernightSwapCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(StartOvernightSwapsProcessCommand),
                    typeof(ChargeSwapsTimeoutInternalCommand)
                )
                .On(DefaultRoute)
                .WithCommandsHandler<OvernightSwapCommandsHandler>()
                .PublishingEvents(
                    typeof(OvernightSwapCalculatedInternalEvent),
                    typeof(OvernightSwapsCalculatedEvent),
                    typeof(OvernightSwapsStartFailedEvent),
                    typeof(OvernightSwapsChargedEvent))
                .With(DefaultPipeline);
        }
        
        private static void RegisterDailyPnlCommandsHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(StartDailyPnlProcessCommand),
                    typeof(ChargeDailyPnlTimeoutInternalCommand)
                )
                .On(DefaultRoute)
                .WithCommandsHandler<DailyPnlCommandsHandler>()
                .PublishingEvents(
                    typeof(DailyPnlCalculatedInternalEvent),
                    typeof(DailyPnlsCalculatedEvent),
                    typeof(DailyPnlsStartFailedEvent),
                    typeof(DailyPnlsChargedEvent))
                .With(DefaultPipeline);
        }

        private IRegistration RegisterAccountListenerSaga()
        {
            var sagaRegistration = RegisterSaga<AccountListenerSaga>();
            
            sagaRegistration
                .ListeningEvents(
                    typeof(AccountChangedEvent))
                .From(_contextNames.AccountsManagement)
                .On(DefaultRoute);

            return sagaRegistration;
        }

        private ISagaRegistration RegisterSaga<TSaga>()
        {
            return Register.Saga<TSaga>($"{_contextNames.CommissionService}.{typeof(TSaga).Name}");
        }
    }
}
using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.Cqrs.Configuration.Routing;
using Lykke.Cqrs.Configuration.Saga;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;
using MarginTrading.CommissionService.Workflow.ChargeCommission;

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
            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory());

            // Sagas & command handlers
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => t.Name.EndsWith("Saga") || t.Name.EndsWith("CommandsHandler"))
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
                RegisterOrderExecSaga(),
                RegisterContext());
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_contextNames.CommissionService)
                .FailedCommandRetryDelay(_defaultRetryDelayMs)
                .ProcessingOptions(DefaultRoute).MultiThreaded(8).QueueCapacity(1024);
            RegisterChargeCommissionCommandHandler(contextRegistration);
            return contextRegistration;
        }

        private PublishingCommandsDescriptor<IDefaultRoutingRegistration> RegisterDefaultRouting()
        {
            return Register.DefaultRouting
                .PublishingCommands(
                    typeof(HandleExecutedOrderInternalCommand))
                .To(_contextNames.CommissionService)
                .With(DefaultPipeline);
        }
        
        private IRegistration RegisterOrderExecSaga()
        {
            var sagaRegistration = RegisterSaga<ChargeCommissionSaga>();
            
            sagaRegistration
                .ListeningEvents(
                    typeof(CommissionCalculatedInternalEvent))
                .From(_contextNames.CommissionService)
                .On(DefaultRoute)
                .PublishingCommands(
                    typeof(ChangeBalanceCommand))
                .To(_contextNames.AccountsManagement)
                .With(DefaultPipeline);

            return sagaRegistration;
        }

        private static void RegisterChargeCommissionCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(HandleExecutedOrderInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<ChargeCommissionCommandsHandler>()
                .PublishingEvents(
                    typeof(CommissionCalculatedInternalEvent))
                .With(DefaultPipeline);
        }

        private ISagaRegistration RegisterSaga<TSaga>()
        {
            return Register.Saga<TSaga>($"{_contextNames.CommissionService}.{typeof(TSaga).Name}");
        }
    }
}
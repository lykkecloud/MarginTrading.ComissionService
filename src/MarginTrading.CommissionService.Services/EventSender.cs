using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using Lykke.MarginTrading.CommissionService.Contracts.Messages;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;
using MarginTrading.SettingsService.Contracts.Enums;
using MarginTrading.SettingsService.Contracts.Messages;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class EventSender : IEventSender
    {
        private readonly IConvertService _convertService;
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IMessageProducer<RateSettingsChangedEvent> _rateSettingsChangedEventProducer;

        public EventSender(
            IRabbitMqService rabbitMqService,
            IConvertService convertService,
            ILog log,
            ISystemClock systemClock,
            ICqrsEngine cqrsEngine,
            CqrsContextNamesSettings contextNames,
            RabbitMqSettings rabbitMqSettings)
        {
            _convertService = convertService;
            _log = log;
            _systemClock = systemClock;
            _cqrsEngine = cqrsEngine;
            _contextNames = contextNames;
            
            _rateSettingsChangedEventProducer =
                rabbitMqService.GetProducer(rabbitMqSettings.Publishers.RateSettingsChanged, true,
                    rabbitMqService.GetJsonSerializer<RateSettingsChangedEvent>());
        }

        public Task SendHandleExecutedOrderInternalCommand(HandleOrderExecInternalCommand command)
        {
            _cqrsEngine.SendCommand(command, 
                _contextNames.CommissionService, 
                _contextNames.CommissionService);
            
            return Task.CompletedTask;
        }

        public Task SendHandleOnBehalfInternalCommand(HandleOnBehalfInternalCommand command)
        {
            _cqrsEngine.SendCommand(command,
                _contextNames.CommissionService, 
                _contextNames.CommissionService);
            
            return Task.CompletedTask;
        }

        public async Task SendRateSettingsChanged(CommissionType type)
        {
            await _rateSettingsChangedEventProducer.ProduceAsync(new RateSettingsChangedEvent
            {
                CreatedTimeStamp = _systemClock.UtcNow.UtcDateTime,
                Type = type.ToType<CommissionTypeContract>()
            });
        }
    }
}
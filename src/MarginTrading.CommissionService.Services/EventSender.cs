using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Messages;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
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
        }

        public async Task SendHandleExecutedOrderInternalCommand(HandleExecutedOrderInternalCommand command)
        {
            _cqrsEngine.SendCommand(command, 
                _contextNames.CommissionService, 
                _contextNames.CommissionService);
        }
    }
}
using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MarginTrading.CommissionService.Contracts.Messages;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
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

        private readonly IMessageProducer<ChargeCommissionMessageContract> _chargeCommissionMessageProducer;

        public EventSender(
            IRabbitMqService rabbitMqService,
            IConvertService convertService,
            ILog log,
            ISystemClock systemClock,
            RabbitMqSettings rabbitMqSettings)
        {
            _convertService = convertService;
            _log = log;
            _systemClock = systemClock;

            _chargeCommissionMessageProducer =
                rabbitMqService.GetProducer(rabbitMqSettings.Publishers.ChargeCommission.ConnectionString, 
                    rabbitMqSettings.Publishers.ChargeCommission.ExchangeName, true,
                    rabbitMqService.GetJsonSerializer<ChargeCommissionMessageContract>());
        }

        public async Task SendChargeCommissionMessage(string operationId, string accountId, 
            CommissionType commissionType, decimal amount)
        {
            var message = new ChargeCommissionMessageContract
            {
                OperationId = operationId,
                CalculationTime = _systemClock.UtcNow.UtcDateTime,
                AccountId = accountId,
                CommissionType = _convertService.Convert<CommissionType, CommissionTypeContract>(commissionType),
                Amount = amount,
            };

            try
            {
                await _chargeCommissionMessageProducer.ProduceAsync(message);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(EventSender), message.ToJson(), ex);
            }
        }
    }
}
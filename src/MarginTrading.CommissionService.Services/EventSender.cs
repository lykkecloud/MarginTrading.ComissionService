using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MarginTrading.CommissionService.Contracts.Messages;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
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

        private readonly IMessageProducer<ChangeBalanceCommand> _chargeCommissionMessageProducer;

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
                    rabbitMqService.GetJsonSerializer<ChangeBalanceCommand>());
        }

        public async Task SendChargeCommissionMessage(string operationId, string clientId, string accountId,
            CommissionType commissionType, decimal amount)
        {
            var message = new ChangeBalanceCommand(operationId, clientId, accountId, amount,
                GetAccountBalanceChangeReasonType(commissionType), $"{commissionType} charge", null);

            try
            {
                await _chargeCommissionMessageProducer.ProduceAsync(message);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(EventSender), message.ToJson(), ex);
            }
        }

        private AccountBalanceChangeReasonTypeContract GetAccountBalanceChangeReasonType(CommissionType commissionType)
        {
            switch (commissionType)
            {
                case CommissionType.OrderExecution:
                    return AccountBalanceChangeReasonTypeContract.RealizedPnL;
                case CommissionType.OnBehalf:
                    return AccountBalanceChangeReasonTypeContract.OnBehalf;
                case CommissionType.OvernightSwap:
                    return AccountBalanceChangeReasonTypeContract.Swap;
                case CommissionType.UnrealizedDailyPnl:
                    return AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(commissionType), commissionType, null);
            }
        }
    }
}
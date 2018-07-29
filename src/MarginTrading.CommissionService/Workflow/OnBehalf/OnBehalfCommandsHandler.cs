using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.OnBehalf
{
    public class OnBehalfCommandsHandler
    {
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IChaosKitty _chaosKitty;
        private readonly ISystemClock _systemClock;

        public OnBehalfCommandsHandler(
            ICommissionCalcService commissionCalcService,
            IChaosKitty chaosKitty,
            ISystemClock systemClock)
        {
            _commissionCalcService = commissionCalcService;
            _chaosKitty = chaosKitty;
            _systemClock = systemClock;
        }
        
        /// <summary>
        /// Calculate commission size
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(HandleOnBehalfInternalCommand command,
            IEventPublisher publisher)
        {
            //todo ensure idempotency
            
            var result = await _commissionCalcService.CalculateOnBehalfCommissionAsync(command.OrderId, 
                command.AccountAssetId);
            
            if (result.Commission == 0)
                return CommandHandlingResult.Ok();
            
            //no failure handling.. so operation will be retried on fail
            
            _chaosKitty.Meow(command.OperationId);
            
            publisher.PublishEvent(new OnBehalfCalculatedInternalEvent(
                operationId: command.OperationId,
                createdTimestamp: _systemClock.UtcNow.UtcDateTime,
                accountId: command.AccountId,
                orderId: command.OrderId,
                assetPairId: command.AssetPairId,
                numberOfActions: result.ActionsNum,
                commission: result.Commission
            ));
            
            return CommandHandlingResult.Ok();
        }
    }
}
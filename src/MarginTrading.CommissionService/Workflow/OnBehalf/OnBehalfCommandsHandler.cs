// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.OnBehalf
{
    public class OnBehalfCommandsHandler
    {
        public const string OperationName = "OnBehalfCommission";
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;

        public OnBehalfCommandsHandler(
            ICommissionCalcService commissionCalcService,
            ISystemClock systemClock, 
            IOperationExecutionInfoRepository executionInfoRepository)
        {
            _commissionCalcService = commissionCalcService;
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
        }
        
        /// <summary>
        /// Calculate commission size
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(HandleOnBehalfInternalCommand command,
            IEventPublisher publisher)
        {
            //ensure idempotency
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName,
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<OnBehalfOperationData>(
                    operationName: OperationName,
                    id: command.OperationId,
                    lastModified: _systemClock.UtcNow.UtcDateTime,
                    data: new OnBehalfOperationData()
                    {
                        AccountId = command.AccountId,
                        OrderId = command.OrderId,
                        State = CommissionOperationState.Initiated,
                    }
                ));

            if (executionInfo?.Data?.State == CommissionOperationState.Initiated)
            {
                var (actionsNum, commission) = await _commissionCalcService.CalculateOnBehalfCommissionAsync(
                    command.OrderId, command.AccountAssetId, command.AssetPairId);

                //no failure handling.. so operation will be retried on fail

                publisher.PublishEvent(new OnBehalfCalculatedInternalEvent(
                    operationId: command.OperationId,
                    createdTimestamp: _systemClock.UtcNow.UtcDateTime,
                    accountId: command.AccountId,
                    orderId: command.OrderId,
                    assetPairId: command.AssetPairId,
                    numberOfActions: actionsNum,
                    commission: commission,
                    tradingDay: command.TradingDay
                ));
            }
        }
    }
}
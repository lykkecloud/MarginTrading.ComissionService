// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICqrsMessageSender
    {
        Task SendHandleExecutedOrderInternalCommand(HandleOrderExecInternalCommand command);
        Task SendHandleOnBehalfInternalCommand(HandleOnBehalfInternalCommand command);

        void PublishEvent<T>(T ev, string boundedContext = null);
    }
}
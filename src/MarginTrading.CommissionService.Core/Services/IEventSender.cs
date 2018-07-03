using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IEventSender
    {
        Task SendHandleExecutedOrderInternalCommand(HandleExecutedOrderInternalCommand command);
    }
}
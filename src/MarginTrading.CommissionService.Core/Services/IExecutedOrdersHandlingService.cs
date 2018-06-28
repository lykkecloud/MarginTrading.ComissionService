using System.Threading.Tasks;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IExecutedOrdersHandlingService
    {
        Task Handle(OrderHistoryEvent orderHistoryEvent);
    }
}
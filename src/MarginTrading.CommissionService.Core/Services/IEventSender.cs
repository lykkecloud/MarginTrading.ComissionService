using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IEventSender
    {
        Task SendChargeCommissionMessage(string operationId, string accountId,
            CommissionType commissionType, decimal amount);
    }
}
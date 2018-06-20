using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IAccountManager
    {
        Task<string> UpdateBalanceAsync(string clientId, string accountId, decimal amount, AccountHistoryType historyType,
            string comment, string eventSourceId = null, bool changeTransferLimit = false, string auditLog = null);
    }
}
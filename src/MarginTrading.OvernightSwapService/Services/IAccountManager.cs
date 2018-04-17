using System.Threading.Tasks;
using MarginTrading.OvernightSwapService.Models;
using MarginTrading.OvernightSwapService.Models.Abstractions;

namespace MarginTrading.OvernightSwapService.Services
{
    public interface IAccountManager
    {
        Task<string> UpdateBalanceAsync(string clientId, string accountId, decimal amount, AccountHistoryType historyType,
            string comment, string eventSourceId = null, bool changeTransferLimit = false, string auditLog = null);
    }
}
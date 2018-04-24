using System.Threading.Tasks;
using MarginTrading.OvernightSwapService.Models;

namespace MarginTrading.OvernightSwapService.Services.Implementation
{
    public class FakeAccountManager : IAccountManager
    {
        //TODO impl update balance event publishing
        public Task<string> UpdateBalanceAsync(string clientId, string accountId, decimal amount, AccountHistoryType historyType,
            string comment, string eventSourceId = null, bool changeTransferLimit = false, string auditLog = null)
        {
            return Task.FromResult("Success");
        }
    }
}
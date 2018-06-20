using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
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
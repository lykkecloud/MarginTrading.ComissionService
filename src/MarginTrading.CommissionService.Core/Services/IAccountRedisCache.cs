using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IAccountRedisCache
    {
        Task<Account> GetAccount(string id);
        Task<IReadOnlyList<Account>> GetAccounts();
    }
}
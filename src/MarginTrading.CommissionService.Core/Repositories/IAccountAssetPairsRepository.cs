using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface IAccountAssetPairsRepository
    {
        Task<IEnumerable<IAccountAssetPair>> GetAllAsync();
    }
}
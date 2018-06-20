using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface IOvernightSwapStateRepository
    {
        Task AddOrReplaceAsync(IOvernightSwapState obj);
        Task<IEnumerable<IOvernightSwapState>> GetAsync();
        Task DeleteAsync(IOvernightSwapState obj);
    }
}
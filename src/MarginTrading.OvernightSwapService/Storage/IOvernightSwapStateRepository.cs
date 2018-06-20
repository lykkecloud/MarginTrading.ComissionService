using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.OvernightSwapService.Models.Abstractions;

namespace MarginTrading.OvernightSwapService.Storage
{
    public interface IOvernightSwapStateRepository
    {
        Task AddOrReplaceAsync(IOvernightSwapState obj);
        Task<IEnumerable<IOvernightSwapState>> GetAsync();
        Task DeleteAsync(IOvernightSwapState obj);
    }
}
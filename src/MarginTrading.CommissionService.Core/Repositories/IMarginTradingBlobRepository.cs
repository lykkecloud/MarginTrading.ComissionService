using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface IMarginTradingBlobRepository
    {
        [CanBeNull]
        T Read<T>(string blobContainer, string key);
        Task WriteAsync<T>(string blobContainer, string key, T obj);
        [ItemCanBeNull]
        Task<T> ReadAsync<T>(string blobContainer, string key);
    }
}
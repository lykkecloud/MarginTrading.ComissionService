using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface IMarginTradingBlobRepository
    {
        [CanBeNull]
        T Read<T>(string blobContainer, string key);
        
        [ItemCanBeNull]
        Task<T> ReadAsync<T>(string blobContainer, string key);
        
        Task WriteAsync<T>(string blobContainer, string key, T obj);
        
        Task MergeListAsync<T>(string blobContainer, string key, List<T> objects, Func<T, string> selector);
        
        Task RemoveFromListAsync<T>(string blobContainer, string key, IEnumerable<T> objects, Func<T, string> selector);
    }
}
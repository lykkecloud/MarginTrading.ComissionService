// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
        Task WriteAsync<T>(string blobContainer, string key, T obj);
        Task MergeListAsync<T>(string blobContainer, string key, List<T> objects, Func<T, string> selector);
        [ItemCanBeNull]
        Task<T> ReadAsync<T>(string blobContainer, string key);
    }
}
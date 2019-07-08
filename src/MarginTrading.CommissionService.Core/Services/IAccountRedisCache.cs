// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
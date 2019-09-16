// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Lykke.MarginTrading.CommissionService.Contracts.Models;

namespace Lykke.MarginTrading.CommissionService.Contracts
{
    public interface ICostsAndChargesApi
    {
        Task<CostsAndChargesCalculationContract> GenerateSingle(string accountId, string instrument, decimal quantity,
            OrderDirectionContract direction, bool withOnBehalf);
        
        Task<CostsAndChargesCalculationContract[]> GenerateForAccount(string accountId, bool withOnBehalf);

        Task<CostsAndChargesCalculationContract[]> GenerateForInstrument(string instrument, bool withOnBehalf);

        Task<PaginatedResponseContract<CostsAndChargesCalculationContract>> Search(string accountId, string
                instrument, decimal? quantity, OrderDirectionContract? direction, DateTime? from, DateTime? to,
            int? skip, int? take, bool isAscendingOrder = true);
        
        Task<CostsAndChargesCalculationContract[]> Get(string[] ids);
    }
}
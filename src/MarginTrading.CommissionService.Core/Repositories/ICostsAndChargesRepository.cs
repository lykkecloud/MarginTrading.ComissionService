// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface ICostsAndChargesRepository
    {
        Task Save(CostsAndChargesCalculation calculation);

        Task<PaginatedResponse<CostsAndChargesCalculation>> Get(string accountId, string instrument, decimal? quantity, 
        OrderDirection? direction, DateTime? from, DateTime? to, int? skip, int? take, bool isAscendingOrder = true);

        Task<CostsAndChargesCalculation[]> GetByIds(string accountId, string[] ids);

        Task<PaginatedResponse<CostsAndChargesCalculation>> GetAllByDay(DateTime date, int? skip, int? take);
    }
}
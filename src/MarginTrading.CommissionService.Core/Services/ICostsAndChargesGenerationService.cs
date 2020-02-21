// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICostsAndChargesGenerationService
    {
        Task<CostsAndChargesCalculation> GenerateSingle(string accountId, string instrument, decimal quantity,
            OrderDirection direction, bool withOnBehalf, decimal? anticipatedExecutionPrice = null);

        Task<SharedCostsAndChargesCalculation> GenerateSharedAsync(string instrument, OrderDirection direction,
            string baseAssetId, string tradingConditionId);

        Task<List<CostsAndChargesCalculation>> GenerateForAccount(string accountId, bool withOnBehalf);

        Task<List<CostsAndChargesCalculation>> GenerateForInstrument(string instrument, bool withOnBehalf);
    }
}
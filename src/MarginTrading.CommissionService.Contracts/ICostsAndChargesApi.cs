// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using Refit;

namespace Lykke.MarginTrading.CommissionService.Contracts
{
    [PublicAPI]
    public interface ICostsAndChargesApi
    {
        [Post("/api/costsAndCharges")]
        Task<CostsAndChargesCalculationContract> GenerateSingle(string accountId, string instrument, decimal quantity,
            OrderDirectionContract direction, bool withOnBehalf, decimal? anticipatedExecutionPrice);

        [Post("/api/costsAndCharges/shared")]
        Task<SharedCostsAndChargesCalculationResult> PrepareShared(string instrument, string tradingConditionId);

        [Post("/api/costsAndCharges/for-account")]
        Task<CostsAndChargesCalculationContract[]> GenerateForAccount(string accountId, bool withOnBehalf);

        [Post("/api/costsAndCharges/for-instrument")]
        Task<CostsAndChargesCalculationContract[]> GenerateForInstrument(string instrument, bool withOnBehalf);

        [Get("/api/costsAndCharges")]
        Task<PaginatedResponseContract<CostsAndChargesCalculationContract>> Search(string accountId, string
                instrument, decimal? quantity, OrderDirectionContract? direction, DateTime? from, DateTime? to,
            int? skip, int? take, bool isAscendingOrder = true);
        
        [Post("/api/costsAndCharges/by-ids")]
        Task<CostsAndChargesCalculationContract[]> GetByIds(string accountId, [Body, CanBeNull] string[] ids);

        [Post("/api/costsAndCharges/pdf-by-day")]
        Task<PaginatedResponseContract<FileContract>> GetByDay(DateTime? date, int? skip, int? take);

        [Post("/api/costsAndCharges/pdf-by-ids")]
        Task<FileContract[]> GenerateBafinCncReport(string accountId, [Body, CanBeNull] string[] ids);

        [Get("/api/costsAndCharges/instruments-with-shared")]
        Task<InstrumentsWithSharedCalculationResult> GetInstrumentsIdsWithExistingSharedFiles(DateTime? date);
    }
}
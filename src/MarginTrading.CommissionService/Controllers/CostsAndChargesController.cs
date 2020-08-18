﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MarginTrading.CommissionService.Contracts;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Middleware;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Controllers
{
    /// <inheritdoc cref="ICostsAndChargesApi" />
    /// Manages costs and charges
    [Authorize]
    [Route("api/costsAndCharges")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class CostsAndChargesController : Controller, ICostsAndChargesApi
    {
        private readonly ICostsAndChargesGenerationService _costsAndChargesGenerationService;
        private readonly ICostsAndChargesRepository _costsAndChargesRepository;
        private readonly ISharedCostsAndChargesRepository _sharedCostsAndChargesRepository;
        private readonly IReportGenService _reportGenService;
        private readonly ISystemClock _systemClock;
        private readonly ILog _log;

        public CostsAndChargesController(
            ICostsAndChargesGenerationService costsAndChargesGenerationService,
            ICostsAndChargesRepository costsAndChargesRepository,
            ISharedCostsAndChargesRepository sharedCostsAndChargesRepository,
            IReportGenService reportGenService,
            ISystemClock systemClock, 
            ILog log)
        {
            _costsAndChargesGenerationService = costsAndChargesGenerationService;
            _costsAndChargesRepository = costsAndChargesRepository;
            _sharedCostsAndChargesRepository = sharedCostsAndChargesRepository;
            _reportGenService = reportGenService;
            _systemClock = systemClock;
            _log = log;
        }

        [ProducesResponseType(typeof(CostsAndChargesCalculationContract), 200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<CostsAndChargesCalculationContract> GenerateSingle(string accountId, string instrument,
            decimal quantity, OrderDirectionContract direction, bool withOnBehalf, decimal? anticipatedExecutionPrice)
        {
            var calculation = await _costsAndChargesGenerationService.GenerateSingle(accountId, instrument, quantity,
                direction.ToType<OrderDirection>(), withOnBehalf, anticipatedExecutionPrice);

            return Map(calculation);
        }

        [Route("shared")]
        [ProducesResponseType(typeof(SharedCostsAndChargesCalculationResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<SharedCostsAndChargesCalculationResult> PrepareShared(string instrument, string tradingConditionId)
        {
            try
            {
                await _costsAndChargesGenerationService.GenerateSharedAsync(instrument, tradingConditionId);
            }
            catch (ArgumentNullException e)
            {
                _log.Error(e, "Invalid input parameters");

                return new SharedCostsAndChargesCalculationResult
                    {Error = SharedCostsAndChargesCalculationError.InvalidInput};
            }

            return new SharedCostsAndChargesCalculationResult {Error = SharedCostsAndChargesCalculationError.None};
        }

        [Route("instruments-with-shared")]
        [ProducesResponseType(typeof(InstrumentsWithSharedCalculationResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(400)]
        [HttpGet]
        public async Task<InstrumentsWithSharedCalculationResult> GetInstrumentsIdsWithExistingSharedFiles(DateTime? 
        date)
        {
            var ids =
                await _sharedCostsAndChargesRepository.GetAssetPairIdsWithFilesAsync(date ?? _systemClock.UtcNow.Date);

            return new InstrumentsWithSharedCalculationResult {InstrumentIds = ids};
        }

        [Route("for-account")]
        [ProducesResponseType(typeof(CostsAndChargesCalculationContract[]), 200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<CostsAndChargesCalculationContract[]> GenerateForAccount(string accountId, bool withOnBehalf)
        {
            var calculations = await _costsAndChargesGenerationService.GenerateForAccount(accountId, withOnBehalf);

            return calculations.Select(Map).ToArray();
        }

        [Route("for-instrument")]
        [ProducesResponseType(typeof(CostsAndChargesCalculationContract[]), 200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<CostsAndChargesCalculationContract[]> GenerateForInstrument(string instrument, bool withOnBehalf)
        {
            var calculations = await _costsAndChargesGenerationService.GenerateForInstrument(instrument, withOnBehalf);

            return calculations.Select(Map).ToArray();
        }

        [Route("")]
        [ProducesResponseType(typeof(PaginatedResponseContract<CostsAndChargesCalculationContract>), 200)]
        [ProducesResponseType(400)]
        [HttpGet]
        public async Task<PaginatedResponseContract<CostsAndChargesCalculationContract>> Search(string accountId, string
         instrument, decimal? quantity, OrderDirectionContract? direction, DateTime? @from,
            DateTime? to, int? skip, int? take, bool isAscendingOrder = false)
        {
            var calculations = await _costsAndChargesRepository.Get(accountId, instrument, quantity, direction?.ToType<OrderDirection>(), from, to, skip, take, isAscendingOrder);

            return new PaginatedResponseContract<CostsAndChargesCalculationContract>(
                calculations.Contents.Select(Map).ToArray(), calculations.Start, calculations.Size,
                calculations.TotalSize);
        }

        [Route("by-ids")]
        [ProducesResponseType(typeof(CostsAndChargesCalculationContract[]), 200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<CostsAndChargesCalculationContract[]> GetByIds(string accountId, [FromBody] string[] ids)
        {
            var calculation = await _costsAndChargesRepository.GetByIds(accountId, ids);

            return calculation.Select(Map).ToArray();
        }

        [Route("pdf-by-ids")]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<FileContract[]> GenerateBafinCncReport(string accountId, [FromBody] string[] ids)
        {
            var calculation = await _costsAndChargesRepository.GetByIds(accountId, ids);

            return calculation.Select(ConvertToFileContract).ToArray();;
        }

        [Route("pdf-by-day")]
        [ProducesResponseType(typeof(CostsAndChargesCalculationContract[]), 200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<PaginatedResponseContract<FileContract>> GetByDay(DateTime? date, int? skip, int? take)
        {
            var response = await _costsAndChargesRepository.GetAllByDay(date ?? _systemClock.UtcNow.Date, skip, take);
            var pdfs = response.Contents.Select(ConvertToFileContract).ToArray();

            return new PaginatedResponseContract<FileContract>(pdfs, response.Start, response.Size, response.TotalSize);
        }

        private FileContract ConvertToFileContract(CostsAndChargesCalculation calculation)
        {
            var accountPrefix = !string.IsNullOrEmpty(calculation.AccountId) ? calculation.AccountId + "_" : "";
            
            return new FileContract
            {
                Name = $"{accountPrefix}{calculation.Instrument}_{calculation.Direction.ToString()}_{calculation.Timestamp:yyyyMMddHHmmssfff}",
                Extension = ".pdf",
                Content = _reportGenService.GenerateBafinCncReport(new[] { calculation })
            };
        }

        private static CostsAndChargesCalculationContract Map(CostsAndChargesCalculation calculation)
        {
            return new CostsAndChargesCalculationContract
            {
                Id = calculation.Id,

                Direction = calculation.Direction.ToType<OrderDirectionContract>(),

                Instrument = calculation.Instrument,

                Timestamp = calculation.Timestamp,

                Volume = calculation.Volume,

                AccountId = calculation.AccountId,

                EntrySum = Map(calculation.EntrySum),

                EntryCost = Map(calculation.EntryCost),

                EntryCommission = Map(calculation.EntryCommission),

                EntryConsorsDonation = Map(calculation.EntryConsorsDonation),

                EntryForeignCurrencyCosts = Map(calculation.EntryForeignCurrencyCosts),

                RunningCostsSum = Map(calculation.RunningCostsSum),

                RunningCostsProductReturnsSum = Map(calculation.RunningCostsProductReturnsSum),

                OvernightCost = Map(calculation.OvernightCost),

                ReferenceRateAmount = Map(calculation.ReferenceRateAmount),

                RepoCost = Map(calculation.RepoCost),

                RunningCommissions = Map(calculation.RunningCommissions),

                RunningCostsConsorsDonation = Map(calculation.RunningCostsConsorsDonation),

                RunningCostsForeignCurrencyCosts = Map(calculation.RunningCostsForeignCurrencyCosts),

                ExitSum = Map(calculation.ExitSum),

                ExitCost = Map(calculation.ExitCost),

                ExitCommission = Map(calculation.ExitCommission),

                ExitConsorsDonation = Map(calculation.ExitConsorsDonation),

                ExitForeignCurrencyCosts = Map(calculation.ExitForeignCurrencyCosts),

                ProductsReturn = Map(calculation.ProductsReturn),

                ServiceCost = Map(calculation.ServiceCost),

                ProductsReturnConsorsDonation = Map(calculation.ProductsReturnConsorsDonation),

                ProductsReturnForeignCurrencyCosts = Map(calculation.ProductsReturnForeignCurrencyCosts),

                TotalCosts = Map(calculation.TotalCosts),

                OneTag = Map(calculation.OneTag)
            };
        }

        private static CostsAndChargesValueContract Map(CostsAndChargesValue value)
        {
            return value == null ? null : new CostsAndChargesValueContract
            {
                ValueInEur = value.ValueInEur,
                ValueInPercent = value.ValueInPercent
            };
        }
    }
}
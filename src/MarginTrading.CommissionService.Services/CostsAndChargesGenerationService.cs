// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    public class CostsAndChargesGenerationService : ICostsAndChargesGenerationService
    {
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IQuoteCacheService _quoteCacheService;
        private readonly IAssetsCache _assetsCache;
        private readonly ISystemClock _systemClock;
        private readonly ICostsAndChargesRepository _repository;
        private readonly IPositionReceiveService _positionReceiveService;

        public CostsAndChargesGenerationService(ICommissionCalcService commissionCalcService,
            IQuoteCacheService quoteCacheService,
            IAssetsCache assetsCache,
            ISystemClock systemClock,
            ICostsAndChargesRepository repository,
            IPositionReceiveService positionReceiveService)
        {
            _commissionCalcService = commissionCalcService;
            _quoteCacheService = quoteCacheService;
            _assetsCache = assetsCache;
            _systemClock = systemClock;
            _repository = repository;
            _positionReceiveService = positionReceiveService;
        }
        
        public async Task<CostsAndChargesCalculation> GenerateSingle(string accountId, string instrument, decimal 
        quantity, OrderDirection direction, bool withOnBehalf)
        {
            var currentBestPrice = _quoteCacheService.GetBidAskPair(instrument);

            var spreadCosts = CalculateSpreadCosts(instrument, quantity, currentBestPrice);
            var executionCommissions = 
                await CalculateCommissions(accountId, instrument, quantity, direction, currentBestPrice);
            var swaps = await CalculateOvernightSwaps(accountId, instrument, quantity, direction, currentBestPrice);

            var calculation = new CostsAndChargesCalculation
            {
                Id = Guid.NewGuid().ToString(),
                Direction = direction,
                Instrument = instrument,
                AccountId = accountId,
                Volume = quantity,
                Timestamp = _systemClock.UtcNow.UtcDateTime,
                EntrySum = new CostsAndChargesValue {ValueInEur = spreadCosts.Entry + executionCommissions.Entry},
                EntryCost = new CostsAndChargesValue {ValueInEur = spreadCosts.Entry},
                EntryCommission = new CostsAndChargesValue {ValueInEur = executionCommissions.Entry},
                EntryConsorsDonation = null,
                EntryForeignCurrencyCosts = new CostsAndChargesValue {ValueInEur = 0},
                RunningCostsSum = null,
                RunningCostsProductReturnsSum = null,
                OvernightCost = new CostsAndChargesValue {ValueInEur = swaps},
                ReferenceRateAmount = null,
                RepoCost = null,
                RunningCommissions = null,
                RunningCostsConsorsDonation = null,
                RunningCostsForeignCurrencyCosts = null,
                ExitSum = null,
                ExitCost = new CostsAndChargesValue {ValueInEur = spreadCosts.Exit},
                ExitCommission = new CostsAndChargesValue {ValueInEur = executionCommissions.Exit},
                ExitConsorsDonation = null,
                ExitForeignCurrencyCosts = null,
                ProductsReturn = null,
                ServiceCost = null,
                ProductsReturnConsorsDonation = null,
                ProductsReturnForeignCurrencyCosts = null,
                TotalCosts = null,
                OneTag = null,
            };
            calculation.SetPercents();

            await _repository.Save(calculation);

            return calculation;
        }

        public async Task<List<CostsAndChargesCalculation>> GenerateForAccount(string accountId, bool withOnBehalf)
        {
            var positions = await _positionReceiveService.GetByAccount(accountId);

            return await GenerateForPositionsList(positions, withOnBehalf);
        }

        public async Task<List<CostsAndChargesCalculation>> GenerateForInstrument(string instrument, bool withOnBehalf)
        {
            var positions = await _positionReceiveService.GetByInstrument(instrument);

            return await GenerateForPositionsList(positions, withOnBehalf);
        }

        private async Task<List<CostsAndChargesCalculation>> GenerateForPositionsList(List<IOpenPosition> positions, bool withOnBehalf)
        {
            var groups = positions.GroupBy(p => (p.AccountId, p.AssetPairId, p.Direction));
            var result = new List<CostsAndChargesCalculation>();

            foreach (var positionsGroup in groups)
            {
                var netVolume = positions.Sum(p => Math.Abs(p.CurrentVolume));
                var direction = positionsGroup.Key.Direction == PositionDirection.Long
                    ? OrderDirection.Sell
                    : OrderDirection.Buy;

                var calculation = await GenerateSingle(positionsGroup.Key.AccountId, positionsGroup.Key.AssetPairId,netVolume, direction, withOnBehalf);
                
                result.Add(calculation);
            }

            return result;
        }

        private (decimal Entry, decimal Exit) CalculateSpreadCosts(string instrument, decimal quantity, 
        InstrumentBidAskPair bestPrice)
        {
            var accuracy = _assetsCache.GetAccuracy(instrument);
            var spread = (bestPrice.Ask - bestPrice.Bid) * quantity;

            var entryCost = Math.Round(spread / 2, accuracy);
            var exitCosts = spread - entryCost;

            return (entryCost, exitCosts);
        }

        private async Task<(decimal Entry, decimal Exit)> CalculateCommissions(string accountId, string instrument, 
        decimal quantity, OrderDirection direction, InstrumentBidAskPair bestPrice)
        {
            decimal openPrice, closePrice;

            if (direction == OrderDirection.Buy)
            {
                openPrice = bestPrice.Ask;
                closePrice = bestPrice.Bid;
            }
            else
            {
                openPrice = bestPrice.Bid;
                closePrice = bestPrice.Ask;
            }

            var openCommission =
                await _commissionCalcService.CalculateOrderExecutionCommission(accountId, instrument, quantity, openPrice);
            
            var closeCommission = await _commissionCalcService.CalculateOrderExecutionCommission(accountId, instrument,quantity, closePrice);

            return (openCommission, closeCommission);
        }

        private async Task<decimal> CalculateOvernightSwaps(string accountId, string instrument, 
            decimal quantity, OrderDirection direction, InstrumentBidAskPair bestPrice)
        {
            var closePrice = direction == OrderDirection.Buy ? bestPrice.Bid : bestPrice.Ask;
            var positionDirection = direction == OrderDirection.Buy ? PositionDirection.Long : PositionDirection.Short;

            var swap = await _commissionCalcService.GetOvernightSwap(accountId, instrument, quantity, closePrice,positionDirection, 1, 365);

            return swap.Swap;
        }
    }
}
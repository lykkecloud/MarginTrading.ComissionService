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
        private readonly ITradingInstrumentsCache _tradingInstrumentsCache;
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly IAccountRedisCache _accountRedisCache;
        private readonly IRateSettingsService _rateSettingsService;
        private readonly IInterestRatesCacheService _interestRatesCacheService;

        public CostsAndChargesGenerationService(ICommissionCalcService commissionCalcService,
            IQuoteCacheService quoteCacheService, 
            IAssetsCache assetsCache,
            ISystemClock systemClock,
            ICostsAndChargesRepository repository,
            IPositionReceiveService positionReceiveService, 
            ITradingInstrumentsCache tradingInstrumentsCache,
            ICfdCalculatorService cfdCalculatorService, 
            IAccountRedisCache accountRedisCache, 
            IRateSettingsService rateSettingsService, IInterestRatesCacheService interestRatesCacheService)
        {
            _commissionCalcService = commissionCalcService;
            _quoteCacheService = quoteCacheService;
            _assetsCache = assetsCache;
            _systemClock = systemClock;
            _repository = repository;
            _positionReceiveService = positionReceiveService;
            _tradingInstrumentsCache = tradingInstrumentsCache;
            _cfdCalculatorService = cfdCalculatorService;
            _accountRedisCache = accountRedisCache;
            _rateSettingsService = rateSettingsService;
            _interestRatesCacheService = interestRatesCacheService;
        }
        
        public async Task<CostsAndChargesCalculation> GenerateSingle(string accountId, string instrument,
            decimal quantity, OrderDirection direction, bool withOnBehalf, decimal? anticipatedExecutionPrice = null)
        {
            var currentBestPrice = _quoteCacheService.GetBidAskPair(instrument);
            anticipatedExecutionPrice = anticipatedExecutionPrice ?? 
                                        (direction == OrderDirection.Buy ? currentBestPrice.Ask : currentBestPrice.Bid);
            var transactionVolume = quantity * anticipatedExecutionPrice.Value;
            
            var account = await _accountRedisCache.GetAccount(accountId);
            var tradingInstrument = _tradingInstrumentsCache.Get(account.TradingConditionId, instrument);
            var fxRate = _cfdCalculatorService.GetFxRateForAssetPair(account.BaseAssetId, instrument, 
                account.LegalEntity);
            var commissionRate = await _rateSettingsService.GetOrderExecutionRate(instrument);
            var overnightSwapRate = await _rateSettingsService.GetOvernightSwapRate(instrument);
            var accuracy = _assetsCache.GetAccuracy(instrument);
            var variableRateBase = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateBase);
            var variableRateQuote = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateQuote);

            var entryConsorsDonation = -(1 - tradingInstrument.HedgeCost)
                                       * (currentBestPrice.Ask - currentBestPrice.Bid) * quantity * fxRate / 4;
            var entryCost = -(currentBestPrice.Ask - currentBestPrice.Bid) * quantity / 2 / fxRate 
                            - entryConsorsDonation;
            var entryCommission = -Math.Min(Math.Max(commissionRate.CommissionFloor, transactionVolume / fxRate),
                                      commissionRate.CommissionCap)
                                  + entryConsorsDonation;
            var overnightCost =
                await CalculateOvernightSwaps(accountId, instrument, quantity, direction, currentBestPrice);
            var referenceRateAmount = -(variableRateBase - variableRateQuote) * transactionVolume / fxRate / 365 * 1; //todo DaysCount always 1 ??
            var repoCost = -overnightSwapRate.RepoSurchargePercent * transactionVolume / fxRate / 365 * 1; //same;
            var runningCostsProductReturnsSum = overnightCost + referenceRateAmount + repoCost;
            var runningCostsConsorsDonation = -(1-tradingInstrument.HedgeCost) 
                                              * overnightSwapRate.FixRate * transactionVolume / fxRate / 365 * 1 / 2;//same
            var runningCommission = runningCostsConsorsDonation;
            var exitConsorsDonation = -(1-tradingInstrument.HedgeCost) * (currentBestPrice.Ask - currentBestPrice.Bid)
                * quantity / fxRate / 2 / 2;
            var exitCost = -(currentBestPrice.Ask - currentBestPrice.Bid) * quantity / 2 / fxRate - exitConsorsDonation;
            var exitCommission = -Math.Min(Math.Max(commissionRate.CommissionFloor, transactionVolume / fxRate),
                                     commissionRate.CommissionCap)
                                 + exitConsorsDonation;
            var productsReturn = entryCost + overnightCost + repoCost + exitCost;
            var serviceCost = entryCommission + runningCommission + exitCommission;
            var productsReturnConsorsDonation = entryConsorsDonation + runningCostsConsorsDonation + exitConsorsDonation;
            var totalCosts = productsReturn + serviceCost + 0;

            var calculation = new CostsAndChargesCalculation
            {
                Id = Guid.NewGuid().ToString(),
                Direction = direction,
                Instrument = instrument,
                AccountId = accountId,
                Volume = quantity,
                Timestamp = _systemClock.UtcNow.UtcDateTime,
                EntrySum = new CostsAndChargesValue {ValueInEur = entryCost + entryCommission},
                EntryCost = new CostsAndChargesValue {ValueInEur = entryCost},
                EntryCommission = new CostsAndChargesValue {ValueInEur = entryCommission},
                EntryConsorsDonation = new CostsAndChargesValue {ValueInEur = entryConsorsDonation},
                EntryForeignCurrencyCosts = new CostsAndChargesValue {ValueInEur = 0},
                RunningCostsSum = new CostsAndChargesValue {ValueInEur = runningCostsProductReturnsSum + runningCostsConsorsDonation},
                RunningCostsProductReturnsSum = new CostsAndChargesValue{ValueInEur = runningCostsProductReturnsSum},
                OvernightCost = new CostsAndChargesValue {ValueInEur = overnightCost},
                ReferenceRateAmount = new CostsAndChargesValue{ValueInEur = referenceRateAmount},
                RepoCost = new CostsAndChargesValue{ValueInEur = repoCost},
                RunningCommissions = new CostsAndChargesValue{ValueInEur = runningCommission},
                RunningCostsConsorsDonation = new CostsAndChargesValue{ValueInEur = runningCostsConsorsDonation},
                RunningCostsForeignCurrencyCosts = new CostsAndChargesValue {ValueInEur = 0},
                ExitSum = new CostsAndChargesValue {ValueInEur = exitCost + exitCommission},
                ExitCost = new CostsAndChargesValue {ValueInEur = exitCost},
                ExitCommission = new CostsAndChargesValue {ValueInEur = exitCommission},
                ExitConsorsDonation = new CostsAndChargesValue{ValueInEur = exitConsorsDonation},
                ExitForeignCurrencyCosts = new CostsAndChargesValue {ValueInEur = 0},
                ProductsReturn = new CostsAndChargesValue {ValueInEur = productsReturn},
                ServiceCost =  new CostsAndChargesValue {ValueInEur = serviceCost},
                ProductsReturnConsorsDonation = new CostsAndChargesValue {ValueInEur = productsReturnConsorsDonation},
                ProductsReturnForeignCurrencyCosts = new CostsAndChargesValue {ValueInEur = 0},
                TotalCosts = new CostsAndChargesValue {ValueInEur = totalCosts},
                OneTag = new CostsAndChargesValue {ValueInEur = totalCosts},
            };
            calculation.Prepare(accuracy);

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

        private async Task<List<CostsAndChargesCalculation>> GenerateForPositionsList(List<IOpenPosition> positions, 
            bool withOnBehalf)
        {
            var groups = positions.GroupBy(p => (p.AccountId, p.AssetPairId, p.Direction));
            var result = new List<CostsAndChargesCalculation>();

            foreach (var positionsGroup in groups)
            {
                var netVolume = positions.Sum(p => Math.Abs(p.CurrentVolume));
                var direction = positionsGroup.Key.Direction == PositionDirection.Long
                    ? OrderDirection.Sell
                    : OrderDirection.Buy;

                var calculation = await GenerateSingle(positionsGroup.Key.AccountId, 
                    positionsGroup.Key.AssetPairId,netVolume, direction, withOnBehalf);
                
                result.Add(calculation);
            }

            return result;
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
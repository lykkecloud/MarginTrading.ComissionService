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
            
            var account = await _accountRedisCache.GetAccount(accountId);
            var tradingInstrument = _tradingInstrumentsCache.Get(account.TradingConditionId, instrument);
            var fxRate = 1 / _cfdCalculatorService.GetFxRateForAssetPair(account.BaseAssetId, instrument, 
                account.LegalEntity);
            var commissionRate = await _rateSettingsService.GetOrderExecutionRate(instrument);
            var overnightSwapRate = await _rateSettingsService.GetOvernightSwapRate(instrument);
            var variableRateBase = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateBase);
            var variableRateQuote = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateQuote);
            var units = 5000 / anticipatedExecutionPrice.Value * fxRate;
            var transactionVolume = units * anticipatedExecutionPrice.Value;
            var spread = currentBestPrice.Ask - currentBestPrice.Bid;

            if (spread == 0)
            {
                spread = currentBestPrice.Ask * tradingInstrument.Spread;
            }
            
            var entryConsorsDonation = -(1 - tradingInstrument.HedgeCost)
                                       * spread * units * fxRate / 4;
            var entryCost = -spread * units / 2 / fxRate 
                            - entryConsorsDonation;
            var entryCommission = -Math.Min(Math.Max(commissionRate.CommissionFloor,
                                          commissionRate.CommissionRate * transactionVolume / fxRate),
                                      commissionRate.CommissionCap) + entryConsorsDonation;
            var overnightCost =
                await CalculateOvernightSwaps(accountId, instrument, units, direction, currentBestPrice);
            var referenceRateAmount = -(variableRateBase - variableRateQuote) * transactionVolume / fxRate / 365 * 1; //todo DaysCount always 1 ??
            var repoCost = -overnightSwapRate.RepoSurchargePercent * transactionVolume / fxRate / 365 * 1; //same;
            var runningCostsProductReturnsSum = overnightCost + referenceRateAmount + repoCost;
            var runningCostsConsorsDonation = -(1-tradingInstrument.HedgeCost) 
                                              * overnightSwapRate.FixRate * transactionVolume / fxRate / 365 * 1 / 2;//same
            var runningCommission = runningCostsConsorsDonation;
            var exitConsorsDonation = -(1-tradingInstrument.HedgeCost) * spread * units / fxRate / 2 / 2;
            var exitCost = -spread * units / 2 / fxRate - exitConsorsDonation;
            var exitCommission = -Math.Min(Math.Max(commissionRate.CommissionFloor,
                                         commissionRate.CommissionRate * transactionVolume / fxRate),
                                     commissionRate.CommissionCap) + exitConsorsDonation;
            var productsReturn = entryCost + overnightCost + repoCost + exitCost;
            var serviceCost = entryCommission + runningCommission + exitCommission;
            var productsReturnConsorsDonation = entryConsorsDonation + runningCostsConsorsDonation + exitConsorsDonation;
            var totalCosts = productsReturn + serviceCost + 0;

            var percentCoef = 1 / transactionVolume * fxRate * 100;

            var calculation = new CostsAndChargesCalculation
            {
                Id = Guid.NewGuid().ToString(),
                Direction = direction,
                Instrument = instrument,
                AccountId = accountId,
                Volume = units,
                Timestamp = _systemClock.UtcNow.UtcDateTime,
                EntrySum = new CostsAndChargesValue(entryCost + entryCommission, 
                    (entryCost + entryCommission) * percentCoef),
                EntryCost = new CostsAndChargesValue(entryCost, entryCost * percentCoef),
                EntryCommission = new CostsAndChargesValue(entryCommission, 
                    entryCommission * percentCoef),
                EntryConsorsDonation = new CostsAndChargesValue(entryConsorsDonation, 
                    entryConsorsDonation * percentCoef),
                EntryForeignCurrencyCosts = new CostsAndChargesValue(0, 0),
                RunningCostsSum = new CostsAndChargesValue(
                    runningCostsProductReturnsSum + runningCostsConsorsDonation,
                    (runningCostsProductReturnsSum + runningCostsConsorsDonation) * percentCoef),
                RunningCostsProductReturnsSum = new CostsAndChargesValue(runningCostsProductReturnsSum,
                    runningCostsProductReturnsSum * percentCoef),
                OvernightCost = new CostsAndChargesValue(overnightCost, 
                    overnightCost * percentCoef),
                ReferenceRateAmount = new CostsAndChargesValue(referenceRateAmount, 
                    referenceRateAmount * percentCoef),
                RepoCost = new CostsAndChargesValue(repoCost, repoCost * percentCoef),
                RunningCommissions = new CostsAndChargesValue(runningCommission, 
                    runningCommission * percentCoef),
                RunningCostsConsorsDonation = new CostsAndChargesValue(runningCostsConsorsDonation,
                    runningCostsConsorsDonation * percentCoef),
                RunningCostsForeignCurrencyCosts = new CostsAndChargesValue(0, 0),
                ExitSum = new CostsAndChargesValue(exitCost + exitCommission,
                    (exitCost + exitCommission) * percentCoef),
                ExitCost = new CostsAndChargesValue(exitCost, exitCost * percentCoef),
                ExitCommission = new CostsAndChargesValue(exitCommission, 
                    exitCommission * percentCoef),
                ExitConsorsDonation = new CostsAndChargesValue(exitConsorsDonation, 
                    exitConsorsDonation * percentCoef),
                ExitForeignCurrencyCosts = new CostsAndChargesValue(0, 0),
                ProductsReturn = new CostsAndChargesValue(productsReturn, 
                    productsReturn * percentCoef),
                ServiceCost =  new CostsAndChargesValue(serviceCost, serviceCost * percentCoef),
                ProductsReturnConsorsDonation = new CostsAndChargesValue(productsReturnConsorsDonation,
                    productsReturnConsorsDonation * percentCoef),
                ProductsReturnForeignCurrencyCosts = new CostsAndChargesValue(0, 0),
                TotalCosts = new CostsAndChargesValue(totalCosts, totalCosts * percentCoef),
                OneTag = new CostsAndChargesValue(totalCosts, totalCosts * percentCoef),
            };
            calculation.RoundValues(2);

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
                var netVolume = positionsGroup.Sum(p => Math.Abs(p.CurrentVolume));
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
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.AssetService.Contracts.ClientProfileSettings;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    public abstract class CostsAndChargesGenerationService : ICostsAndChargesGenerationService
    {
        private readonly IQuoteCacheService _quoteCacheService;
        private readonly ISystemClock _systemClock;
        private readonly ICostsAndChargesRepository _repository;
        private readonly ISharedCostsAndChargesRepository _sharedRepository;
        private readonly IPositionReceiveService _positionReceiveService;
        private readonly ITradingInstrumentsCache _tradingInstrumentsCache;
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly IAccountRedisCache _accountRedisCache;
        private readonly IRateSettingsCache _rateSettingsCache;
        private readonly IInterestRatesCacheService _interestRatesCacheService;
        private readonly IAssetPairsCache _assetPairsCache;
        private readonly ITradingDaysInfoProvider _tradingDaysInfoProvider;
        private readonly IBrokerSettingsService _brokerSettingsService;
        private readonly CostsAndChargesDefaultSettings _defaultSettings;
        private readonly CommissionServiceSettings _settings;
        private readonly IClientProfileSettingsCache _clientProfileSettingsCache;

        private readonly decimal _defaultCcVolume;
        private readonly decimal _donationShare;

        public CostsAndChargesGenerationService(IQuoteCacheService quoteCacheService,
            ISystemClock systemClock,
            ICostsAndChargesRepository repository,
            ISharedCostsAndChargesRepository sharedRepository,
            IPositionReceiveService positionReceiveService,
            ITradingInstrumentsCache tradingInstrumentsCache,
            ICfdCalculatorService cfdCalculatorService,
            IAccountRedisCache accountRedisCache,
            IRateSettingsCache rateSettingsCache,
            IInterestRatesCacheService interestRatesCacheService,
            IAssetPairsCache assetPairsCache,
            ITradingDaysInfoProvider tradingDaysInfoProvider,
            IBrokerSettingsService brokerSettingsService,
            CostsAndChargesDefaultSettings defaultSettings,
            CommissionServiceSettings settings,
            decimal defaultCcVolume,
            decimal donationShare)
        {
            _quoteCacheService = quoteCacheService;
            _systemClock = systemClock;
            _repository = repository;
            _positionReceiveService = positionReceiveService;
            _tradingInstrumentsCache = tradingInstrumentsCache;
            _cfdCalculatorService = cfdCalculatorService;
            _accountRedisCache = accountRedisCache;
            _rateSettingsCache = rateSettingsCache;
            _interestRatesCacheService = interestRatesCacheService;
            _assetPairsCache = assetPairsCache;
            _tradingDaysInfoProvider = tradingDaysInfoProvider;
            _brokerSettingsService = brokerSettingsService;
            _defaultSettings = defaultSettings;
            _settings = settings;
            _clientProfileSettingsCache = clientProfileSettingsCache;
            _sharedRepository = sharedRepository;
            _defaultCcVolume = defaultCcVolume;
            _donationShare = donationShare;
        }

        public async Task<CostsAndChargesCalculation> GenerateSingle(string accountId, string instrument,
            decimal quantity, OrderDirection direction, bool withOnBehalf, decimal? anticipatedExecutionPrice = null)
        {
            var account = await _accountRedisCache.GetAccount(accountId);

            var calculation = await GetCalculationAsync(instrument, direction,
                account.BaseAssetId, account
                    .TradingConditionId, account.LegalEntity, anticipatedExecutionPrice);

            calculation.AccountId = accountId;

            await _repository.Save(calculation);

            return calculation;
        }

        public async Task<List<CostsAndChargesCalculation>> GenerateSharedAsync(string instrument, string
            tradingConditionId)
        {
            var result = new List<CostsAndChargesCalculation>();
            var settlementCurrency = await _brokerSettingsService.GetSettlementCurrencyAsync();

            foreach (var direction in new[] {OrderDirection.Buy, OrderDirection.Sell})
            {
                var calculation = await GetCalculationAsync(instrument, direction, settlementCurrency,
                    tradingConditionId, _defaultSettings.LegalEntity);

                await _sharedRepository.SaveAsync(calculation);

                result.Add(calculation);
            }

            return result;
        }

        protected virtual async Task<CostsAndChargesCalculation> GetCalculationAsync(string instrument,
            OrderDirection direction,
            string baseAssetId,
            string tradingConditionId,
            string legalEntity,
            decimal? anticipatedExecutionPrice = null)
        {
            if (string.IsNullOrEmpty(instrument))
                throw new ArgumentNullException(nameof(instrument));

            if (string.IsNullOrEmpty(baseAssetId))
                throw new ArgumentNullException(nameof(baseAssetId));

            if (string.IsNullOrEmpty(tradingConditionId))
                throw new ArgumentNullException(nameof(tradingConditionId));

            var currentBestPrice = _quoteCacheService.GetBidAskPair(instrument);
            var executionPrice = anticipatedExecutionPrice ??
                                 (direction == OrderDirection.Buy ? currentBestPrice.Ask : currentBestPrice.Bid);

            var tradingInstrument = _tradingInstrumentsCache.Get(tradingConditionId, instrument);
            var fxRate = 1 / _cfdCalculatorService.GetFxRateForAssetPair(baseAssetId, instrument, legalEntity);
            var overnightSwapRate = await _rateSettingsCache.GetOvernightSwapRate(instrument, tradingConditionId);
            var units = _defaultCcVolume / executionPrice * fxRate;
            var transactionVolume = units * executionPrice;
            var spread = currentBestPrice.Ask - currentBestPrice.Bid;

            if (spread == 0)
            {
                spread = tradingInstrument.Spread;
            }

            var assetPair = _assetPairsCache.GetAssetPairById(instrument);
            var entryBrokerDonation = -(1 - tradingInstrument.HedgeCost) * spread * units / fxRate / 2 * _donationShare;
            var entryCost = -spread * units / 2 / fxRate - entryBrokerDonation;
            var clientProfileSettings = _clientProfileSettingsCache.GetByIds(tradingConditionId, assetPair.AssetType);
            var commissionBase = -Math.Min(
                Math.Max(
                    clientProfileSettings.ExecutionFeesFloor,
                    clientProfileSettings.ExecutionFeesRate * transactionVolume / fxRate),
                clientProfileSettings.ExecutionFeesCap);
            var entryCommission = commissionBase + entryBrokerDonation;

            var overnightFeeDays = _tradingDaysInfoProvider.GetNumberOfNightsUntilNextTradingDay(assetPair.MarketId,
                _systemClock.UtcNow.UtcDateTime);
           
            var runningCostsConsorsDonation = -1 * overnightSwapRate.FixRate * transactionVolume / fxRate / 365 *
                    overnightFeeDays * _donationShare;

            var runningCosts = -1 * overnightSwapRate.FixRate * transactionVolume / fxRate / 365 *
                               overnightFeeDays - runningCostsConsorsDonation;
            var directionMultiplier = direction == OrderDirection.Sell ? -1 : 1;

            var referenceRateAmount = 0m;

            if (!_settings.AssetTypesWithZeroInterestRates.Contains(assetPair.AssetType))
            {
                var variableRateBase = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateBase);
                var variableRateQuote = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateQuote);

                referenceRateAmount = directionMultiplier * (variableRateBase - variableRateQuote) *
                    transactionVolume / fxRate / 365 * overnightFeeDays;
            }

            var repoCost = direction == OrderDirection.Sell
                ? -overnightSwapRate.RepoSurchargePercent * transactionVolume / fxRate / 365 * overnightFeeDays
                : 0;
            var runningCostsProductReturnsSum = runningCosts + referenceRateAmount + repoCost;

            var runningCommission = runningCostsConsorsDonation;
            var exitBrokerDonation = -(1 - tradingInstrument.HedgeCost) * spread * units / fxRate / 2 * _donationShare;
            var exitCost = -spread * units / 2 / fxRate - exitBrokerDonation;
            var exitCommission = commissionBase + exitBrokerDonation;
            var productsReturn = entryCost + runningCostsProductReturnsSum + exitCost;
            var serviceCost = entryCommission + runningCommission + exitCommission;
            var productsReturnConsorsDonation =
                entryBrokerDonation + runningCostsConsorsDonation + exitBrokerDonation;
            var totalCosts = productsReturn + serviceCost + 0;

            var percentCoef = 1 / (transactionVolume / fxRate) * 100;

            var calculation = new CostsAndChargesCalculation
            {
                Id = Guid.NewGuid().ToString(),
                Direction = direction,
                Instrument = instrument,
                InstrumentName = assetPair.Name,
                BaseAssetId = baseAssetId,
                TradingConditionId = tradingConditionId,
                LegalEntity = legalEntity,
                Volume = units,
                Timestamp = _systemClock.UtcNow.UtcDateTime,
                EntrySum = new CostsAndChargesValue(entryCost + entryCommission,
                    (entryCost + entryCommission) * percentCoef),
                EntryCost = new CostsAndChargesValue(entryCost, entryCost * percentCoef),
                EntryCommission = new CostsAndChargesValue(entryCommission,
                    entryCommission * percentCoef),
                EntryConsorsDonation = new CostsAndChargesValue(entryBrokerDonation,
                    entryBrokerDonation * percentCoef),
                EntryForeignCurrencyCosts = new CostsAndChargesValue(0, 0),
                RunningCostsSum = new CostsAndChargesValue(
                    runningCostsProductReturnsSum + runningCostsConsorsDonation,
                    (runningCostsProductReturnsSum + runningCostsConsorsDonation) * percentCoef),
                RunningCostsProductReturnsSum = new CostsAndChargesValue(runningCostsProductReturnsSum,
                    runningCostsProductReturnsSum * percentCoef),
                OvernightCost = new CostsAndChargesValue(runningCostsConsorsDonation,
                    runningCostsConsorsDonation * percentCoef),
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
                ExitConsorsDonation = new CostsAndChargesValue(exitBrokerDonation,
                    exitBrokerDonation * percentCoef),
                ExitForeignCurrencyCosts = new CostsAndChargesValue(0, 0),
                ProductsReturn = new CostsAndChargesValue(productsReturn,
                    productsReturn * percentCoef),
                ServiceCost = new CostsAndChargesValue(serviceCost, serviceCost * percentCoef),
                ProductsReturnConsorsDonation = new CostsAndChargesValue(productsReturnConsorsDonation,
                    productsReturnConsorsDonation * percentCoef),
                ProductsReturnForeignCurrencyCosts = new CostsAndChargesValue(0, 0),
                TotalCosts = new CostsAndChargesValue(totalCosts, totalCosts * percentCoef),
                OneTag = new CostsAndChargesValue(totalCosts, totalCosts * percentCoef),
                OnBehalfFee = _rateSettingsService.GetOnBehalfRate(assetPair.AssetType).Commission,
                EntryExitCommission = new CostsAndChargesValue(entryCommission + exitCommission, 
                    (entryCommission + exitCommission) * percentCoef),
                EntryExitCost = new CostsAndChargesValue(entryCost + exitCost, 
                    (entryCost + exitCost) * percentCoef),
            };
            calculation.RoundValues(2);

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
                    positionsGroup.Key.AssetPairId, netVolume, direction, withOnBehalf);

                result.Add(calculation);
            }

            return result;
        }
    }
}
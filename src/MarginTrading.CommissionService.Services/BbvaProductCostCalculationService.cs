// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class BbvaProductCostCalculationService : IProductCostCalculationService
    {
        private readonly IRateSettingsCache _rateSettingsCache;
        private readonly IInterestRatesCacheService _interestRatesCacheService;
        private readonly IQuoteCacheService _quoteCacheService;

        public BbvaProductCostCalculationService(IRateSettingsCache
                rateSettingsCache,
            IInterestRatesCacheService interestRatesCacheService,
            IQuoteCacheService quoteCacheService)
        {
            _rateSettingsCache = rateSettingsCache;
            _interestRatesCacheService = interestRatesCacheService;
            _quoteCacheService = quoteCacheService;
        }

        public decimal EntryCost(decimal ask, decimal bid, decimal transactionVolume, decimal fxRate)
        {
            var spread = ask - bid;
            return -spread * transactionVolume / 2 / fxRate;
        }

        public decimal ExitCost(decimal ask, decimal bid, decimal transactionVolume, decimal fxRate)
        {
            var spread = ask - bid;
            return -spread * transactionVolume / 2 / fxRate;
        }

        public decimal RunningOvernightCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays)
        {
            return -1 * overnightSwapRate.FixRate * transactionVolume / fxRate / 365 *
                   overnightFeeDays;
        }

        public decimal ReferenceRateAmountInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction)
        {
            var directionMultiplier = direction == OrderDirection.Sell ? -1 : 1;

            var variableRateBase = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateBase);
            var variableRateQuote = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateQuote);

            return directionMultiplier * (variableRateBase - variableRateQuote) *
                transactionVolume / fxRate / 365 * overnightFeeDays;
        }

        public decimal RepoCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction)
        {
            return direction == OrderDirection.Sell
                ? -overnightSwapRate.RepoSurchargePercent * transactionVolume / fxRate / 365 * overnightFeeDays
                : 0;
        }

        public async Task<decimal> RunningProductCost(string productId,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction, 
            string tradingConditionId)
        {
            var overnightSwapRate = await _rateSettingsCache.GetOvernightSwapRate(productId, tradingConditionId);

            var runningOvernightCostInEUR =
                RunningOvernightCostInEUR(overnightSwapRate, transactionVolume, fxRate, overnightFeeDays);
            var referenceRateAmountInEUR = ReferenceRateAmountInEUR(overnightSwapRate, transactionVolume, fxRate,
                overnightFeeDays, direction);
            var repoCostInEUR = RepoCostInEUR(overnightSwapRate, transactionVolume, fxRate,
                overnightFeeDays, direction);

            return runningOvernightCostInEUR + referenceRateAmountInEUR + repoCostInEUR;
        }

        public async Task<decimal> ProductCost(string productId,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction,
            string tradingConditionId)
        {
            var currentBestPrice = _quoteCacheService.GetBidAskPair(productId);
            var entryCost = EntryCost(currentBestPrice.Ask, currentBestPrice.Bid, transactionVolume, fxRate);
            var runningCost =
                await RunningProductCost(productId, transactionVolume, fxRate, overnightFeeDays, direction, tradingConditionId);

            var exitCost = ExitCost(currentBestPrice.Ask, currentBestPrice.Bid, transactionVolume, fxRate);

            return entryCost + runningCost + exitCost;
        }
    }
}
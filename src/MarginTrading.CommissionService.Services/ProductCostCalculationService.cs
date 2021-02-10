// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class ProductCostCalculationService : IProductCostCalculationService
    {
        private readonly IRateSettingsService _rateSettingsService;
        private readonly IInterestRatesCacheService _interestRatesCacheService;

        public ProductCostCalculationService(IRateSettingsService rateSettingsService,
            IInterestRatesCacheService interestRatesCacheService)
        {
            _rateSettingsService = rateSettingsService;
            _interestRatesCacheService = interestRatesCacheService;
        }

        public decimal RunningOvernightCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays)
        {
            // todo: wrong for consors
            return -1 * overnightSwapRate.FixRate * transactionVolume / fxRate / 365 *
                overnightFeeDays ;
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

        public async Task<decimal> ProductCost(string productId,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction)
        {
            var overnightSwapRate = await _rateSettingsService.GetOvernightSwapRate(productId);

            var runningOvernightCostInEUR =
                RunningOvernightCostInEUR(overnightSwapRate, transactionVolume, fxRate, overnightFeeDays);
            var referenceRateAmountInEUR = ReferenceRateAmountInEUR(overnightSwapRate, transactionVolume, fxRate,
                overnightFeeDays, direction);
            var repoCostInEUR = RepoCostInEUR(overnightSwapRate, transactionVolume, fxRate,
                overnightFeeDays, direction);

            return runningOvernightCostInEUR + referenceRateAmountInEUR + repoCostInEUR;
        }
    }
}
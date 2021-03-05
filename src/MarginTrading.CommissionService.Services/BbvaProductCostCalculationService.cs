// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class BbvaProductCostCalculationService : IProductCostCalculationService
    {
        public decimal EntryCost(decimal spread, decimal transactionVolume, decimal fxRate)
        {
            return -spread * transactionVolume / 2 / fxRate;
        }

        public decimal ExitCost(decimal spread, decimal transactionVolume, decimal fxRate)
        {
            return -spread * transactionVolume / 2 / fxRate;
        }

        public decimal RunningOvernightCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays)
        {
            return -1 * overnightSwapRate.FixRate * transactionVolume / fxRate / 365 *
                   overnightFeeDays ;
        }

        public decimal ReferenceRateAmountInEUR(decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction)
        {
            var directionMultiplier = direction == OrderDirection.Sell ? -1 : 1;

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

        public decimal RunningProductCost(OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction)
        {
            var runningOvernightCostInEUR =
                RunningOvernightCostInEUR(overnightSwapRate, transactionVolume, fxRate, overnightFeeDays);
            var referenceRateAmountInEUR = ReferenceRateAmountInEUR(transactionVolume,
                fxRate,
                overnightFeeDays,
                variableRateBase,
                variableRateQuote,
                direction);
            var repoCostInEUR = RepoCostInEUR(overnightSwapRate, transactionVolume, fxRate,
                overnightFeeDays, direction);

            return runningOvernightCostInEUR + referenceRateAmountInEUR + repoCostInEUR;
        }

        public decimal ProductCost(decimal spread,
            OvernightSwapRate swapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction)
        {
            var entryCost = EntryCost(spread, transactionVolume, fxRate);
            var runningCost = RunningProductCost(swapRate,
                transactionVolume,
                fxRate,
                overnightFeeDays,
                variableRateBase,
                variableRateQuote,
                direction);

            var exitCost = ExitCost(spread, transactionVolume, fxRate);

            return entryCost + runningCost + exitCost;
        }
    }
}
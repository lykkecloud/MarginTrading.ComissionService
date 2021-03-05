// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.


using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IProductCostCalculationService
    {
        decimal EntryCost(decimal spread, decimal transactionVolume, decimal fxRate);
        decimal ExitCost(decimal spread, decimal transactionVolume, decimal fxRate);

        decimal RunningOvernightCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays);

        decimal ReferenceRateAmountInEUR(decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction);

        decimal RepoCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction);

        decimal RunningProductCost(OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction);

        decimal ProductCost(decimal spread,
            OvernightSwapRate swapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction);
    }
}
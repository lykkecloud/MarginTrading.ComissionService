// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.


using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IProductCostCalculationService
    {
        decimal EntryCost(decimal spread, decimal quantity, decimal fxRate);
        decimal ExitCost(decimal spread, decimal quantity, decimal fxRate);

        decimal RunningOvernightCostInEUR(
            decimal financingFeeRate,
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
            decimal financingFeeRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction);

        decimal ProductCost(decimal spread,
            OvernightSwapRate swapRate,
            decimal quantity,
            decimal financingFeeRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction);

        decimal ExecutedOrderEntryCost(decimal spreadWeight, decimal fxRate);
        decimal ExecutedOrderExitCost(decimal spreadWeight, decimal fxRate);

        decimal ExecutedOrderProductCost(decimal spreadWeight,
            OvernightSwapRate swapRate,
            decimal financingFeeRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            decimal variableRateBase,
            decimal variableRateQuote,
            OrderDirection direction);
    }
}
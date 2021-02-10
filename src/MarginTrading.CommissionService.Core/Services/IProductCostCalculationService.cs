// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.


using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IProductCostCalculationService
    {
        decimal RunningOvernightCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays);

        decimal ReferenceRateAmountInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction);

        decimal RepoCostInEUR(
            OvernightSwapRate overnightSwapRate,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction);

        Task<decimal> ProductCost(string productId,
            decimal transactionVolume,
            decimal fxRate,
            int overnightFeeDays,
            OrderDirection direction);
    }
}
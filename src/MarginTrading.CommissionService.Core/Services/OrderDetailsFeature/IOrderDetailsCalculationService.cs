// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;

namespace MarginTrading.CommissionService.Core.Services.OrderDetailsFeature
{
    public interface IOrderDetailsCalculationService
    {
        Task<OrderDetailsData> Calculate(string orderId, string accountId);
    }
}
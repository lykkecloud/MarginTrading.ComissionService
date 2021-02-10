// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Contracts.Models;
using Refit;

namespace MarginTrading.CommissionService.Contracts
{
    public interface IOrderDetailsApi
    {
        [Post("api/orderDetails")]
        Task<FileContract> GenerateOrderDetailsReport([Query] string orderId, [Query] string accountId);
    }
}
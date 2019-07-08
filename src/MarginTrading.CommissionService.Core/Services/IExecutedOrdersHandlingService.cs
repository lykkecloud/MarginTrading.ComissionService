// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IExecutedOrdersHandlingService
    {
        Task Handle(OrderHistoryEvent orderHistoryEvent);
    }
}
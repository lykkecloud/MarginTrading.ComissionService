// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public enum OrderStatus
    {
        Placed,
        Inactive,
        Active,
        ExecutionStarted,
        Executed,
        Canceled,
        Rejected,
        Expired,
    }
}
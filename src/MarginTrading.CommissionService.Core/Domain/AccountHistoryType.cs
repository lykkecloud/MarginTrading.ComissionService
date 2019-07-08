// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public enum AccountHistoryType
    {
        Deposit = 1,
        Withdraw = 2,
        OrderClosed = 3,
        Reset = 4,
        Swap = 5,
        Manual = 6,
    }
}
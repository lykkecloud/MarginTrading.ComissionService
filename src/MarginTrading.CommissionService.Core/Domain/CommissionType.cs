// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public enum CommissionType
    {
        OrderExecution = 1,
        OnBehalf = 2,
        OvernightSwap = 3,
        UnrealizedDailyPnl = 4,
    }
}
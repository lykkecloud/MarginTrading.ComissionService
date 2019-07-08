// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public enum CommissionOperationState
    {
        Initiated = 0,
        Calculated = 1,
        Succeeded = 2,
        Failed = 3,
    }
}
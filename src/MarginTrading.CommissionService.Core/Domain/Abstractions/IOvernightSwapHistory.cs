using System;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IOvernightSwapHistory : IOvernightSwapState
    {
        bool IsSuccess { get; }
        Exception Exception { get; }
    }
}
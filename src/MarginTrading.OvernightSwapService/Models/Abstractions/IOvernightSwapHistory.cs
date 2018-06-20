using System;

namespace MarginTrading.OvernightSwapService.Models.Abstractions
{
    public interface IOvernightSwapHistory : IOvernightSwapState
    {
        bool IsSuccess { get; }
        Exception Exception { get; }
    }
}
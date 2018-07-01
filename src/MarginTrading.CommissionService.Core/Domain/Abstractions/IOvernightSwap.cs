using System;
using System.Collections.Generic;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IOvernightSwap
    {
        string ClientId { get; }
        string AccountId { get; }
        string Instrument { get; }
        PositionDirection? Direction { get; }
        DateTime Time { get; }
        decimal Volume { get; }
        decimal Value { get; }
        decimal SwapRate { get; }
        List<string> OpenOrderIds { get; }
        bool IsSuccess { get; }
        Exception Exception { get; }
    }
}
using System;
using System.Collections.Generic;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IOvernightSwapState
    {
        string ClientId { get; }
        string AccountId { get; }
        string Instrument { get; }
        OrderDirection? Direction { get; }
        DateTime Time { get; }
        decimal Volume { get; }
        decimal Value { get; }
        decimal SwapRate { get; }
        List<string> OpenOrderIds { get; }
    }
}
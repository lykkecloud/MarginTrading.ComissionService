using System;
using System.Collections.Generic;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IOvernightSwapCalculation
    {
        string Id { get; }
        string OperationId { get; }
        string AccountId { get; }
        string Instrument { get; }
        PositionDirection? Direction { get; }
        DateTime Time { get; }
        decimal Volume { get; }
        decimal SwapValue { get; }
        string PositionId { get; }
        string Details { get; }
        DateTime TradingDay { get; }
        
        bool IsSuccess { get; }
        Exception Exception { get; }
        
        bool? WasCharged { get; }
    }
}
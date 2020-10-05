// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

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
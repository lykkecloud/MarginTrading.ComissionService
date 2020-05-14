// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IDailyPnlCalculation
    {
        string Id { get; }
        
        string OperationId { get; }
        string AccountId { get; }
        string Instrument { get; }
        DateTime Time { get; }
        DateTime TradingDay { get; }
        decimal Volume { get; }
        decimal FxRate { get; }
        string PositionId { get; }
        decimal Pnl { get; }
        
        bool IsSuccess { get; }
        Exception Exception { get; }
        
        bool? WasCharged { get; }
        decimal RawTotalPnl { get; }
    }
}
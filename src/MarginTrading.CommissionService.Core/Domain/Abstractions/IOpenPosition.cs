// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IOpenPosition
    {
        string Id { get; }
        string AccountId { get; }
        string AssetPairId { get; }
        DateTime OpenTimestamp { get; }
        PositionDirection Direction { get; }
        decimal OpenPrice { get; }
        decimal? ExpectedOpenPrice { get; }
        decimal ClosePrice { get; }
        decimal CurrentVolume { get; }
        decimal PnL { get; }
        decimal UnrealizedPnl { get; }
        decimal Margin { get; }
        decimal FxRate { get; }
        string TradeId { get; }
        List<string> RelatedOrders { get; }
        List<RelatedOrderInfo> RelatedOrderInfos { get; }
        decimal ChargedPnl { get; set; }
    }
}
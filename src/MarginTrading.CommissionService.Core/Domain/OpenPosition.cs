// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class OpenPosition : IOpenPosition
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string AssetPairId { get; set; }
        public DateTime OpenTimestamp { get; set; }
        public PositionDirection Direction { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal? ExpectedOpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CurrentVolume { get; set; }
        public decimal PnL { get; set; }
        public decimal UnrealizedPnl { get; set; }
        public decimal ChargedPnl { get; set; }
        public decimal Margin { get; set; }
        public decimal FxRate { get; set; }
        public string TradeId { get; set; }
        public List<string> RelatedOrders { get; set; }
        public List<RelatedOrderInfo> RelatedOrderInfos { get; set; }
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.Core.Domain.OrderDetailFeature
{
    public class OrderDetailsData
    {
        public string Instrument { get; set; }
        public decimal Quantity { get; set; }
        
        public OrderStatus Status { get; set; }
        public OrderType OrderType { get; set; }
        public decimal? LimitStopPrice { get; set; }
        public decimal? TakeProfitPrice { get; set; }
        public decimal? StopLossPrice { get; set; }
        public decimal? ExecutionPrice { get; set; }
        public decimal? Notional { get; set; }
        public decimal? NotionalEUR { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal? ProductCost { get; set; }
        public OrderDirection OrderDirection { get; set; }
        public OriginatorType Origin { get; set; }
        public string OrderId { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime ModifiedTimestamp { get; set; }
        public DateTime? ExecutedTimestamp { get; set; }
        public DateTime? CanceledTimestamp { get; set; }
        
        public DateTime? ValidityTime { get; set; }
        public string OrderComment { get; set; }
        public bool ForceOpen { get; set; }
        public decimal? Commission { get; set; }
        public decimal? TotalCostsAndCharges { get; set; }
        
        /// <summary>
        /// Indicates that user had to manually confirm that they understand trading risks
        /// </summary>
        public bool ConfirmedManually { get; set; }

        public string AccountName { get; set; }

        public string SettlementCurrency { get; set; }
        
        public decimal? MoreThan5Percent { get; set; }
        
        public decimal? LossRatioFrom { get; set; }
        
        public decimal? LossRatioTo { get; set; }
    }
}
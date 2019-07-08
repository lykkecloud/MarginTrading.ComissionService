// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class OvernightSwapHistoryContract
    {
        public string Id { get; set; }
        public string OperationId { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public PositionDirectionContract? Direction { get; set; }
        public DateTime Time { get; set; }
        public decimal Volume { get; set; }
        public decimal SwapValue { get; set; }
        public string PositionId { get; set; }
        public string Details { get; set; }
        public DateTime TradingDay { get; set; }
        
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
        
        /// <summary>
        /// Null - not charged yet, False - charging failed, True - charged
        /// </summary>
        public bool? WasCharged { get; set; }
    }
}
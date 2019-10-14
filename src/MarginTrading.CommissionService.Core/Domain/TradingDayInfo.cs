// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class TradingDayInfo
    {
        /// <summary>
        /// Trading day that was active last
        /// </summary>
        public DateTime LastTradingDay { get; set; }
        
        /// <summary>
        ///  Timestamp when next trading day with start
        /// </summary>
        public DateTime NextTradingDayStart { get; set; }
    }
}
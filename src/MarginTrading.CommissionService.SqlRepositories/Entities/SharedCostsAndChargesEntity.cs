// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.SqlRepositories.Entities
{
    public class SharedCostsAndChargesEntity
    {
        public string Id { get; set; }
        
        public string Instrument { get; set; }
        
        public string BaseAssetId { get; set; }
        
        public string TradingConditionId { get; set; }
        
        public string LegalEntity { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public decimal Volume { get; set; }
        
        public string Direction { get; set; }
        
        public string Data { get; set; }
    }
}
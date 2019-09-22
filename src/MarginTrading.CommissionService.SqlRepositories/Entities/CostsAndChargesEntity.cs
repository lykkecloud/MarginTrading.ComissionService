// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.SqlRepositories.Entities
{
    public class CostsAndChargesEntity
    {
        public string Id { get; set; }
        
        public string AccountId { get; set; }
        
        public string Instrument { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public decimal Volume { get; set; }
        
        public string Direction { get; set; }
        
        public string Data { get; set; }
    }
}
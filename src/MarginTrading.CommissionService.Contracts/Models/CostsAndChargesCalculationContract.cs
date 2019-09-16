// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class CostsAndChargesCalculationContract
    {
        public string Id { get; set; }
        
        public string AccountId { get; set; }
        
        public string Instrument { get; set; }
        
        public decimal Volume { get; set; }
        
        public OrderDirectionContract Direction { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public decimal EntryCost { get; set; }
        
        public decimal ExitCost { get; set; }
        
        public decimal EntryCommission { get; set; }
        
        public decimal ExitCommission { get; set; }
        
        public decimal OvernightCost { get; set; }
    }
}
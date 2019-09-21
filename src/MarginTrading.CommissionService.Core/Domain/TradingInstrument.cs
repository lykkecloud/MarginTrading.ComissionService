// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public class TradingInstrument
    {
        public string TradingConditionId { get; set; }
        public string Instrument { get; set; }
        
        public decimal HedgeCost { get; set; }
        
        public (string, string) GetKey() => (TradingConditionId, Instrument);
    }
}
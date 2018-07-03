using MarginTrading.CommissionService.Core.Domain.CommissionRates.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain.CommissionRates
{
    public class OvernightSwapRate : IOvernightSwapRate
    {
        //comes from TradingInstrument
        public string TradingConditionId { get; set; }
        public string Instrument { get; set; }
        
        public decimal FixRate { get; set; }
        public decimal VariableRate1 { get; set; }
        public decimal VariableRate2 { get; set; } //ask Elena about it
        public string ChargingCurrency => "EUR";
    }
}
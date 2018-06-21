namespace MarginTrading.CommissionService.Core.Domain.CommissionRates.Abstractions
{
    public interface IOvernightSwapRate
    {
        string TradingConditionId { get; }
        string Instrument { get; }

        decimal FixRate { get; }
        decimal VariableRate1 { get; }
        decimal VariableRate2 { get; }
        string ChargingCurrency { get; }
    }
}
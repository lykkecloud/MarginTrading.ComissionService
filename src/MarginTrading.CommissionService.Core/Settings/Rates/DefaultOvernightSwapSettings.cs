namespace MarginTrading.CommissionService.Core.Settings.Rates
{
    public class DefaultOvernightSwapSettings
    {
        
        public decimal RepoSurchargePercent { get; set; }
        
        public decimal FixRate { get; set; }
        
        public string VariableRateBase { get; set; }
        
        public string VariableRateQuote { get; set; }
    }
}
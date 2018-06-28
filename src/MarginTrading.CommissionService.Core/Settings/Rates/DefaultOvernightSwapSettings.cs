namespace MarginTrading.CommissionService.Core.Settings.Rates
{
    public class DefaultOvernightSwapSettings
    {
        
        public decimal RepoSurchargePercent { get; set; }
        
        public decimal FixRate { get; set; }
        
        public decimal VariableRateBase { get; set; }
        
        public decimal VariableRateQuote { get; set; }
        
        public string CommissionAsset { get; set; }
    }
}
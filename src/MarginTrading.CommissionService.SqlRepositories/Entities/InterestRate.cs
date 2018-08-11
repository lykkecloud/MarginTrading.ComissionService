using System;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.SqlRepositories.Entities
{
    public class InterestRate : IInterestRate
    {
        public string MdsCode { get; set; }
        string IInterestRate.AssetPairId => MdsCode;
        
        public decimal ClosePrice { get; set; }
        decimal IInterestRate.Rate => ClosePrice;
        
        public DateTime Timestamp { get; set; }
    }
}
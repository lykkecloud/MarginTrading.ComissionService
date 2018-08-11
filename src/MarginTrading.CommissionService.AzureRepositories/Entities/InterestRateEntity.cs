using System;
using Lykke.AzureStorage.Tables;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.CommissionService.AzureRepositories.Entities
{
    public class InterestRateEntity : AzureTableEntity,  IInterestRate
    {
        public string MdsCode { get; set; }
        string IInterestRate.AssetPairId => MdsCode;
        
        public decimal ClosePrice { get; set; }
        decimal IInterestRate.Rate => ClosePrice;
        
        public DateTime Timestamp { get; set; }
    }
}
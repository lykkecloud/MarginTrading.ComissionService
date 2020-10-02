// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Lykke.AzureStorage.Tables;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

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
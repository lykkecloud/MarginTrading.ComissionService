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
        
        public CostsAndChargesValueContract EntrySum { get; set; }
        
        public CostsAndChargesValueContract EntryCost { get; set; }
        
        public CostsAndChargesValueContract EntryCommission { get; set; }
        
        public CostsAndChargesValueContract EntryConsorsDonation { get; set; }
        
        public CostsAndChargesValueContract EntryForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValueContract RunningCostsSum { get; set; }
        
        public CostsAndChargesValueContract RunningCostsProductReturnsSum { get; set; }
        
        public CostsAndChargesValueContract OvernightCost { get; set; }
        
        public CostsAndChargesValueContract ReferenceRateAmount { get; set; }
        
        public CostsAndChargesValueContract RepoCost { get; set; }
        
        public CostsAndChargesValueContract RunningCommissions { get; set; }
        
        public CostsAndChargesValueContract RunningCostsConsorsDonation { get; set; }
        
        public CostsAndChargesValueContract RunningCostsForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValueContract ExitSum { get; set; }
        
        public CostsAndChargesValueContract ExitCost { get; set; }
        
        public CostsAndChargesValueContract ExitCommission { get; set; }
        
        public CostsAndChargesValueContract ExitConsorsDonation { get; set; }
        
        public CostsAndChargesValueContract ExitForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValueContract ProductsReturn { get; set; }
        
        public CostsAndChargesValueContract ServiceCost { get; set; }
        
        public CostsAndChargesValueContract ProductsReturnConsorsDonation { get; set; }
        
        public CostsAndChargesValueContract ProductsReturnForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValueContract TotalCosts { get; set; }
        
        public CostsAndChargesValueContract OneTag { get; set; }
    }
}
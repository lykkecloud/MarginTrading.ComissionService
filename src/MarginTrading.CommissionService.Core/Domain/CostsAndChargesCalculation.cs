// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class CostsAndChargesCalculation
    {
        public string Id { get; set; }
        
        public string AccountId { get; set; }
        
        public string Instrument { get; set; }
        
        public decimal Volume { get; set; }
        
        public OrderDirection Direction { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public CostsAndChargesValue EntrySum { get; set; }
        
        public CostsAndChargesValue EntryCost { get; set; }
        
        public CostsAndChargesValue EntryCommission { get; set; }
        
        public CostsAndChargesValue EntryConsorsDonation { get; set; }
        
        public CostsAndChargesValue EntryForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValue RunningCostsSum { get; set; }
        
        public CostsAndChargesValue RunningCostsProductReturnsSum { get; set; }
        
        public CostsAndChargesValue OvernightCost { get; set; }
        
        public CostsAndChargesValue ReferenceRateAmount { get; set; }
        
        public CostsAndChargesValue RepoCost { get; set; }
        
        public CostsAndChargesValue RunningCommissions { get; set; }
        
        public CostsAndChargesValue RunningCostsConsorsDonation { get; set; }
        
        public CostsAndChargesValue RunningCostsForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValue ExitSum { get; set; }
        
        public CostsAndChargesValue ExitCost { get; set; }
        
        public CostsAndChargesValue ExitCommission { get; set; }
        
        public CostsAndChargesValue ExitConsorsDonation { get; set; }
        
        public CostsAndChargesValue ExitForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValue ProductsReturn { get; set; }
        
        public CostsAndChargesValue ServiceCost { get; set; }
        
        public CostsAndChargesValue ProductsReturnConsorsDonation { get; set; }
        
        public CostsAndChargesValue ProductsReturnForeignCurrencyCosts { get; set; }
        
        public CostsAndChargesValue TotalCosts { get; set; }
        
        public CostsAndChargesValue OneTag { get; set; }


        public void RoundValues(int accuracy)
        {
            foreach (var propertyInfo in typeof(CostsAndChargesCalculation).GetProperties()
                .Where(x => x.PropertyType == typeof(CostsAndChargesValue)))
            {
                var property = (CostsAndChargesValue)propertyInfo.GetValue(this);
                property.ValueInEur = Math.Round(property.ValueInEur, accuracy);
                property.ValueInPercent = Math.Round(property.ValueInPercent, accuracy);
            }
        }
    }
}
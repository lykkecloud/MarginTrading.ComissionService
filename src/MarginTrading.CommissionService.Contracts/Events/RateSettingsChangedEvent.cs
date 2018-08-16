using System;
using Lykke.MarginTrading.CommissionService.Contracts.Models;

namespace Lykke.MarginTrading.CommissionService.Contracts.Events
{
    public class RateSettingsChangedEvent
    {
        public DateTime CreatedTimeStamp { get; set; }
        
        public CommissionTypeContract Type { get; set; }
    }
}
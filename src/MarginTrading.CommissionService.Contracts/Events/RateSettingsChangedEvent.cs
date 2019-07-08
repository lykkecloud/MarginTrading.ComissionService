// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
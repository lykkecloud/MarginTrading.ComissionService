// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MessagePack;

namespace MarginTrading.CommissionService.Contracts.Commands
{
    [MessagePackObject]
    public class ChargeDailyPnlTimeoutInternalCommand
    {
        [Key(0)]
        public string OperationId { get; set; }
        
        [Key(1)]
        public DateTime CreationTime { get; set; }
        
        [Key(2)]
        public int TimeoutSeconds { get; set; }
    }
}
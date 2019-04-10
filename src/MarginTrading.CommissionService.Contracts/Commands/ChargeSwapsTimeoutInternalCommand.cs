using System;
using MessagePack;

namespace Lykke.MarginTrading.CommissionService.Contracts.Commands
{
    [MessagePackObject]
    public class ChargeSwapsTimeoutInternalCommand
    {
        [Key(0)]
        public string OperationId { get; set; }
        
        [Key(1)]
        public DateTime CreationTime { get; set; }
        
        [Key(2)]
        public int TimeoutSeconds { get; set; }
    }
}
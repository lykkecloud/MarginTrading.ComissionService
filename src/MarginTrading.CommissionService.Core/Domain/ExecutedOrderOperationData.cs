// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public class ExecutedOrderOperationData : OperationDataBase<CommissionOperationState>
    {
        public string AccountId { get; set; }
        
        public string OrderId { get; set; }
        
        public long OrderCode { get; set; }
        
        public string Instrument { get; set; }
        
        public string LegalEntity { get; set; }
        
        public decimal Volume { get; set; }
    }
}
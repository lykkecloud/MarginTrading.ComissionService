using System;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Domain
{
    [MessagePackObject]
    public class Account
    {
        [Key(0)]
        public string Id { get; }

        [Key(1)]
        public string ClientId { get; }

        [Key(2)]
        public string TradingConditionId { get; }

        [Key(3)]
        public string BaseAssetId { get; }

        [Key(4)]
        public decimal Balance { get; }

        [Key(5)]
        public decimal WithdrawTransferLimit { get; }

        [Key(6)]
        public string LegalEntity { get; }

        [Key(7)]
        public bool IsDisabled { get; }

        [Key(8)]
        public DateTime ModificationTimestamp { get; }
    }
}
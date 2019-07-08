// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Domain
{
    [MessagePackObject]
    public class Account
    {
        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public string ClientId { get; set; }

        [Key(2)]
        public string TradingConditionId { get; set; }

        [Key(3)]
        public string BaseAssetId { get; set; }

        [Key(4)]
        public decimal Balance { get; set; }

        [Key(5)]
        public decimal WithdrawTransferLimit { get; set; }

        [Key(6)]
        public string LegalEntity { get; set; }

        [Key(7)]
        public bool IsDisabled { get; set; }

        [Key(8)]
        public DateTime ModificationTimestamp { get; set; }
    }
}
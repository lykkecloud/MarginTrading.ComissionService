// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.OrderDetailFeature
{
    public class ReportProperties
    {
        public string OrderId { get; set; }
        public string AccountId { get; set; }
        public bool IncludeManualConfirmationFooter { get; set; }
    }
}
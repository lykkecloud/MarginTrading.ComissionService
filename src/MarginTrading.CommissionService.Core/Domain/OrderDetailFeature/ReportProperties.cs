// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.OrderDetailFeature
{
    public class ReportProperties
    {
        public string OrderId { get; set; }

        public string AccountName { get; set; }

        public bool HasManualConfirmationWarning { get; set; }

        public string ProductName { get; set; }

        public bool HasMoreThan5PercentWarning { get; set; }

        public string MoreThan5PercentWarning { get; set; }

        public bool HasLossRatioWarning { get; set; }

        public string LossRatioFrom { get; set; }

        public string LossRatioTo { get; set; }
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.OrderDetailFeature
{
    public class ReportProperties
    {
        public string OrderId { get; set; }

        public string AccountName { get; set; }

        public bool EnableProductComplexityWarning { get; set; }

        public string ProductName { get; set; }

        public bool EnableTotalCostPercentWarning { get; set; }

        public string TotalCostPercentWarning { get; set; }

        public bool EnableLossRatioWarning { get; set; }

        public string LossRatioMin { get; set; }

        public string LossRatioMax { get; set; }

        /// <summary>
        /// Enables / disables all warnings globally
        /// </summary>
        public bool EnableAllWarnings { get; set; }
    }
}
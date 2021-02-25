// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.OrderDetailFeature
{
    public class OrderDetailsReportRow
    {
        public OrderDetailsReportCell LeftGroup { get; set; }
        public OrderDetailsReportCell RightGroup { get; set; }

        public OrderDetailsReportRow(OrderDetailsReportCell left, OrderDetailsReportCell right)
        {
            LeftGroup = left;
            RightGroup = right;
        }

        public OrderDetailsReportRow(string leftName, string leftValue, string rightName, string rightValue)
        {
            LeftGroup = new OrderDetailsReportCell(leftName, leftValue);
            RightGroup = new OrderDetailsReportCell(rightName, rightValue);
        }
    }
}
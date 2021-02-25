// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace MarginTrading.CommissionService.Core.Domain.OrderDetailFeature
{
    public class OrderDetailsReport
    {
        public ReportProperties Properties { get; set; }

        public IReadOnlyCollection<OrderDetailsReportRow> Data { get; set; }
        
    }
}
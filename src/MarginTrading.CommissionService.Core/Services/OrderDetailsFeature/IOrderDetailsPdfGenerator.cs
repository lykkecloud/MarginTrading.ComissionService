// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;

namespace MarginTrading.CommissionService.Core.Services.OrderDetailsFeature
{
    public interface IOrderDetailsPdfGenerator
    {
        byte[] GenerateReport(IReadOnlyCollection<OrderDetailsReportRow> data, ReportProperties properties);
    }
}
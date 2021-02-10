// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Core.Services.OrderDetailsFeature;

namespace MarginTrading.CommissionService.Services.OrderDetailsFeature
{
    public class OrderDetailsDataSourceBuilder : IOrderDetailsDataSourceBuilder
    {
        private readonly IOrderDetailsLocalizationService _localizationService;

        public OrderDetailsDataSourceBuilder(IOrderDetailsLocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public IReadOnlyCollection<OrderDetailsReportRow> Build(OrderDetailsData data)
        {
            var result = new List<OrderDetailsReportRow>();

            var row1 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.Instrument)),
                data.Instrument,
                _localizationService.LocalizeField(nameof(OrderDetailsData.OrderDirection)),
                _localizationService.LocalizeDirection(data.OrderDirection)
            );

            var row2 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.Quantity)),
                _localizationService.LocalizeQuantity(data.Quantity, data.OrderDirection),
                _localizationService.LocalizeField(nameof(OrderDetailsData.Origin)),
                _localizationService.LocalizeOrigin(data.Origin)
            );

            var row3 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.OrderType)),
                _localizationService.LocalizeOrderType(data.OrderType),
                _localizationService.LocalizeField(nameof(OrderDetailsData.OrderId)),
                data.OrderId
            );

            var row4 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.Status)),
                _localizationService.LocalizeOrderStatus(data.Status),
                _localizationService.LocalizeField(nameof(OrderDetailsData.CreatedTimestamp)),
                _localizationService.LocalizeDate(data.CreatedTimestamp)
            );

            var row5 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.LimitStopPrice)),
                _localizationService.LocalizeDecimal(data.LimitStopPrice),
                _localizationService.LocalizeField(nameof(OrderDetailsData.ModifiedTimestamp)),
                _localizationService.LocalizeDate(data.ModifiedTimestamp)
            );
            
            var row6 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.TakeProfitPrice)),
                _localizationService.LocalizeDecimal(data.TakeProfitPrice),
                _localizationService.LocalizeField(nameof(OrderDetailsData.ExecutedTimestamp)),
                _localizationService.LocalizeDate(data.ExecutedTimestamp)
            );  
            
            var row7 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.StopLossPrice)),
                _localizationService.LocalizeDecimal(data.StopLossPrice),
                _localizationService.LocalizeField(nameof(OrderDetailsData.CanceledTimestamp)),
                _localizationService.LocalizeDate(data.CanceledTimestamp)
            );   
            
            var row8 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.ExecutionPrice)),
                _localizationService.LocalizeDecimal(data.ExecutionPrice),
                _localizationService.LocalizeField(nameof(OrderDetailsData.ValidityTime)),
                _localizationService.LocalizeValidity(data.ValidityTime)
            );
            
            var row9 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.Notional)),
                _localizationService.LocalizeDecimal(data.Notional),
                _localizationService.LocalizeField(nameof(OrderDetailsData.OrderComment)),
                data.OrderComment
            );
            
            var row10 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.NotionalEUR)),
                _localizationService.LocalizeDecimal(data.NotionalEUR),
                _localizationService.LocalizeField(nameof(OrderDetailsData.ForceOpen)),
                _localizationService.LocalizeBoolean(data.ForceOpen)
            );
            
            var row11 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.ExchangeRate)),
                _localizationService.LocalizeDecimal(data.ExchangeRate),
                _localizationService.LocalizeField(nameof(OrderDetailsData.Commission)),
                _localizationService.LocalizeDecimal(data.Commission)
            );
            
            var row12 = new OrderDetailsReportRow(
                _localizationService.LocalizeField(nameof(OrderDetailsData.ProductCost)),
                _localizationService.LocalizeDecimal(data.ProductCost),
                _localizationService.LocalizeField(nameof(OrderDetailsData.TotalCostsAndCharges)),
                _localizationService.LocalizeDecimal(data.TotalCostsAndCharges)
            );

            result.Add(row1);
            result.Add(row2);
            result.Add(row3);
            result.Add(row4);
            result.Add(row5);
            result.Add(row6);
            result.Add(row7);
            result.Add(row8);
            result.Add(row9);
            result.Add(row10);
            result.Add(row11);
            result.Add(row12);

            return result;
        }
    }
}
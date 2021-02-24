// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services.OrderDetailsFeature
{
    public interface IOrderDetailsLocalizationService
    {
        string LocalizeField(string field);
        string LocalizeDirection(OrderDirection orderDirection);
        
        string LocalizeDecimal(decimal? value, int? precision = null);
        string LocalizeExchangeRate(decimal? value);
        string LocalizeOrigin(OriginatorType origin);
        string LocalizeQuantity(decimal? quantity, OrderDirection direction);
        string LocalizeOrderType(OrderType orderType);
        string LocalizeOrderStatus(OrderStatus status);
        string LocalizeDate(DateTime? dateTime);
        string LocalizeValidity(DateTime? validityTime);
        string LocalizeBoolean(bool value);
    }
}
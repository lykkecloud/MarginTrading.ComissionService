// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services.OrderDetailsFeature;

namespace MarginTrading.CommissionService.Services.OrderDetailsFeature
{
    public class OrderDetailsEnglishLocalizationService : IOrderDetailsLocalizationService
    {
        private string empty = "-";
        
        public string LocalizeField(string field)
        {
            return field;
        }

        public string LocalizeDirection(OrderDirection orderDirection)
        {
            return orderDirection.ToString("G");
        }

        public string LocalizeDecimal(decimal? value, int? precision = null)
        {
            return value.HasValue ? value.ToString() : empty;
        }

        public string LocalizeOrigin(OriginatorType origin)
        {
            return origin.ToString("G");
        }

        public string LocalizeQuantity(decimal? quantity, OrderDirection direction)
        {
            if (quantity.HasValue)
            {
                var sign = direction == OrderDirection.Buy ? "+" : "";
                return $"{sign}{quantity.Value.ToString("G29", CultureInfo.InvariantCulture)}";
            }

            return empty;
        }

        public string LocalizeOrderType(OrderType orderType)
        {
            return orderType.ToString("G");
        }

        public string LocalizeOrderStatus(OrderStatus status)
        {
            return status.ToString("G");
        }

        public string LocalizeDate(DateTime? dateTime)
        {
            return dateTime.HasValue
                ? dateTime.Value.ToString(CultureInfo.InvariantCulture)
                : empty;
        }
        
        public string LocalizeValidity(DateTime? validityTime)
        {
            return validityTime.HasValue
                ? validityTime.Value.ToString(CultureInfo.InvariantCulture)
                : "GTC";
        }

        public string LocalizeBoolean(bool value)
        {
            return value.ToString();
        }
    }
}
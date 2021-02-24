// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Core.Services.OrderDetailsFeature;

namespace MarginTrading.CommissionService.Services.OrderDetailsFeature
{
    public class OrderDetailsSpanishLocalizationService : IOrderDetailsLocalizationService
    {
        private string empty = "-";

        private readonly CultureInfo _cultureInfo = new CultureInfo("es-ES");
        private string _dateTimeFormat = "dd.MM.yyyy hh:mm:ss";
        private string _dateFormat = "dd.MM.yyyy";
        private const string decimalFormat = "G29";

        private readonly Dictionary<string, string> _fieldMap = new Dictionary<string, string>()
        {
            {nameof(OrderDetailsData.Instrument), "Instrumento"},
            {nameof(OrderDetailsData.OrderDirection), "Sentido"},
            {nameof(OrderDetailsData.Quantity), "Cantidad"},
            {nameof(OrderDetailsData.Origin), "Origen"},
            {nameof(OrderDetailsData.OrderType), "Tipo de orden"},
            {nameof(OrderDetailsData.OrderId), "ID Orden"},
            {nameof(OrderDetailsData.Status), "Estado de orden"},
            {nameof(OrderDetailsData.CreatedTimestamp), "Fecha de creación"},
            {nameof(OrderDetailsData.LimitStopPrice), "Límite/Stop"},
            {nameof(OrderDetailsData.ModifiedTimestamp), "Fecha de modificación"},
            {nameof(OrderDetailsData.TakeProfitPrice), "Toma de Beneficios"},
            {nameof(OrderDetailsData.ExecutedTimestamp), "Fecha de ejecución"},
            {nameof(OrderDetailsData.StopLossPrice), "Stop de pérdidas"},
            {nameof(OrderDetailsData.CanceledTimestamp), "Fecha de cancelación"},
            {nameof(OrderDetailsData.ExecutionPrice), "Precio de ejecución"},
            {nameof(OrderDetailsData.ValidityTime), "Validez"},
            {nameof(OrderDetailsData.Notional), "Efectivo"},
            {nameof(OrderDetailsData.OrderComment), "Comentario de la orden"},
            {nameof(OrderDetailsData.NotionalEUR), "Efectivo"},
            {nameof(OrderDetailsData.ExchangeRate), "Tipo de cambio"},
            {nameof(OrderDetailsData.Commission), "Comisión de compra/venta"},
            {nameof(OrderDetailsData.ProductCost), "Coste del producto (Spread oferta/demanda)"},
            {nameof(OrderDetailsData.TotalCostsAndCharges), "Total costes y gastos"},
            {nameof(OrderDetailsData.ForceOpen), "Forzar apertura"},
        };

        private readonly Dictionary<OrderDirection, string> _orderDirectionMap = new Dictionary<OrderDirection, string>()
        {
            {OrderDirection.Buy, "Comprar"},
            {OrderDirection.Sell, "Vender"},
        };

        private readonly Dictionary<OrderType, string> _orderTypeMap = new Dictionary<OrderType, string>()
        {
            {OrderType.Limit, "Límite"},
            {OrderType.Market, "Mercado"},
            {OrderType.Stop, "Stop"},
            {OrderType.StopLoss, "Stop de pérdidas"},
            {OrderType.TakeProfit, "Toma de beneficios"},
            {OrderType.TrailingStop, "Stop de pérdidas"},
        };

        private readonly Dictionary<OrderStatus, string> _orderStatusMap = new Dictionary<OrderStatus, string>()
        {
            {OrderStatus.Executed, "Ejecutada"},
            {OrderStatus.Canceled, "Cancelada"},
            {OrderStatus.Expired, "Caducada"},
            {OrderStatus.Rejected, "Rechazada"},
        };

        private readonly Dictionary<OriginatorType, string> _originatorMap = new Dictionary<OriginatorType, string>()
        {
            {OriginatorType.Investor, "Orden de cliente"},
            {OriginatorType.OnBehalf, "Orden telefónica"},
            {OriginatorType.System, "Orden automática"},
        };

        public string LocalizeField(string field)
        {
            var success = _fieldMap.TryGetValue(field, out var value);
            return success ? value : empty;
        }

        public string LocalizeDirection(OrderDirection orderDirection)
        {
            var success = _orderDirectionMap.TryGetValue(orderDirection, out var value);
            return success ? value : empty;
        }

        public string LocalizeDecimal(decimal? value, int? precision = null)
        {
            var format = precision == null ? decimalFormat : $"N{precision}";
            if (value.HasValue)
            {
                return $"{value.Value.ToString(format, _cultureInfo)}";
            }

            return empty;
        }

        public string LocalizeQuantity(decimal? quantity, OrderDirection direction)
        {
            if (quantity.HasValue)
            {
                var sign = direction == OrderDirection.Buy ? "+" : "";
                return $"{sign}{quantity.Value.ToString(decimalFormat, _cultureInfo)}";
            }

            return empty;
        }

        public string LocalizeOrderType(OrderType orderType)
        {
            var success = _orderTypeMap.TryGetValue(orderType, out var value);
            return success ? value : empty;
        }

        public string LocalizeOrderStatus(OrderStatus status)
        {
            var success = _orderStatusMap.TryGetValue(status, out var value);
            return success ? value : empty;
        }

        public string LocalizeDate(DateTime? dateTime)
        {
            return dateTime.HasValue
                ? dateTime.Value.ToString(_dateTimeFormat)
                : empty;
        }

        public string LocalizeValidity(DateTime? validityTime)
        {
            return validityTime.HasValue
                ? validityTime.Value.ToString(_dateFormat)
                : "GTC";
        }

        public string LocalizeBoolean(bool value)
        {
            return value ? "Sí" : "No";
        }

        public string LocalizeOrigin(OriginatorType origin)
        {
            var success = _originatorMap.TryGetValue(origin, out var value);
            return success ? value : empty;
        }
    }
}
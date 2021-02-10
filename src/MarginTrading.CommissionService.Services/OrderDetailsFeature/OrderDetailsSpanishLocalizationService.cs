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

        public string LocalizeField(string field)
        {
            var map = new Dictionary<string, string>()
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
                {nameof(OrderDetailsData.NotionalEUR), "Efectivo (EUR)"},
                {nameof(OrderDetailsData.ExchangeRate), "Tipo de cambio"},
                {nameof(OrderDetailsData.Commission), "Comisión de compra/venta"},
                {nameof(OrderDetailsData.ProductCost), "Coste del producto (Spread oferta/demanda)"},
                {nameof(OrderDetailsData.TotalCostsAndCharges), "Total costes y gastos"},
                {nameof(OrderDetailsData.ForceOpen), "Forzar apertura"},
            };

            var success = map.TryGetValue(field, out var value);
            return success ? value : empty;
        }

        public string LocalizeDirection(OrderDirection orderDirection)
        {
            var map = new Dictionary<OrderDirection, string>()
            {
                {OrderDirection.Buy, "Comprar"},
                {OrderDirection.Sell, "Vender"},
            };

            var success = map.TryGetValue(orderDirection, out var value);
            return success ? value : empty;
        }

        public string LocalizeDecimal(decimal? value)
        {
            if (value.HasValue)
            {
                return $"{value.Value.ToString("G", new CultureInfo("es-ES"))}";
            }

            return empty;
        }

        public string LocalizeQuantity(decimal? quantity, OrderDirection direction)
        {
            if (quantity.HasValue)
            {
                var sign = direction == OrderDirection.Buy ? "+" : "-";
                return $"{sign}{quantity.Value.ToString("N0", new CultureInfo("es-ES"))}";
            }

            return empty;
        }

        public string LocalizeOrderType(OrderType orderType)
        {
            var map = new Dictionary<OrderType, string>()
            {
                {OrderType.Limit, "Límite"},
                {OrderType.Market, "Mercado"},
                {OrderType.Stop, "Stop"},
                {OrderType.StopLoss, "Stop de pérdidas"},
                {OrderType.TakeProfit, "Toma de beneficios"},
                {OrderType.TrailingStop, "Stop de pérdidas"},
            };
            var success = map.TryGetValue(orderType, out var value);
            return success ? value : empty;
        }

        public string LocalizeOrderStatus(OrderStatus status)
        {
            var map = new Dictionary<OrderStatus, string>()
            {
                {OrderStatus.Executed, "Ejecutada"},
                {OrderStatus.Canceled, "Cancelada"},
                {OrderStatus.Expired, "Caducada"},
                {OrderStatus.Rejected, "Rechazada"},
            };
            var success = map.TryGetValue(status, out var value);
            return success ? value : empty;
        }

        public string LocalizeDate(DateTime? dateTime)
        {
            return dateTime.HasValue
                ? dateTime.Value.ToString("dd.MM.yyyy hh:mm:ss")
                : empty;
        }

        public string LocalizeValidity(DateTime? validityTime)
        {
            return validityTime.HasValue
                ? validityTime.Value.ToString("dd.MM.yyyy hh:mm:ss")
                : "GTC";
        }

        public string LocalizeBoolean(bool value)
        {
            return value ? "Sí" : "No";
        }

        public string LocalizeOrigin(OriginatorType origin)
        {
            var map = new Dictionary<OriginatorType, string>()
            {
                {OriginatorType.Investor, "Orden de cliente"},
                {OriginatorType.OnBehalf, "Orden telefónica"},
                {OriginatorType.System, "Orden automática"},
            };

            var success = map.TryGetValue(origin, out var value);
            return success ? value : empty;
        }
    }
}
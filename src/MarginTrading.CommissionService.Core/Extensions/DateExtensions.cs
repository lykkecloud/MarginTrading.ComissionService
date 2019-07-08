// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Common.Log;

namespace MarginTrading.CommissionService.Core.Extensions
{
    public static class DateExtensions
    {
        public static DateTime ValidateTradingDay(this DateTime tradingDay, ILog log, string context)
        {
            if (tradingDay != tradingDay.Date)
            {
                log.WriteWarning(nameof(ValidateTradingDay), context, $"Trading day {tradingDay:s} contained not only Date component, truncating.");
            }

            return tradingDay.Date;
        }
    }
}
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
                log.WriteWarning(nameof(ValidateTradingDay), context, $"{tradingDay:s}");
            }

            return tradingDay.Date;
        }
    }
}
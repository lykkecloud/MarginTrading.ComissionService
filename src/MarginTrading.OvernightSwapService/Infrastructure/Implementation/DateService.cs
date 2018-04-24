using System;

namespace MarginTrading.OvernightSwapService.Infrastructure.Implementation
{
    public class DateService : IDateService
    {
        public DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
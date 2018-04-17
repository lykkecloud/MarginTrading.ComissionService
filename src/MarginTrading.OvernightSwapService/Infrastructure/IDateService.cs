using System;

namespace MarginTrading.OvernightSwapService.Infrastructure
{
    public interface IDateService
    {
        DateTime Now();
    }
}
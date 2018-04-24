using System;

namespace MarginTrading.OvernightSwapService.Services
{
    public interface IOvernightSwapNotificationService
    {
        void PerformEmailNotification(DateTime calculationTime);
    }
}
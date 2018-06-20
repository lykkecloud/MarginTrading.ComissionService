using System;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IOvernightSwapNotificationService
    {
        void PerformEmailNotification(DateTime calculationTime);
    }
}
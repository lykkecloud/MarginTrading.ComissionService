using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Cqrs;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IDailyPnlListener
    {
        Task DailyPnlStateChanged(string operationId, bool chargedOrFailed);
    }
}
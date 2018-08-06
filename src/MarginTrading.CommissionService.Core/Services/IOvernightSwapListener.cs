using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Cqrs;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IOvernightSwapListener
    {
        Task TrackCharging(string operationId, IEnumerable<string> operationIds, IEventPublisher publisher);
    }
}
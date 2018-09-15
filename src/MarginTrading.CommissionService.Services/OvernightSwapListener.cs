using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using MarginTrading.CommissionService.Core.Domain.EventArgs;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class OvernightSwapListener: IOvernightSwapListener,
        IEventConsumer<OvernightSwapChargedEventArgs>, IEventConsumer<OvernightSwapChargeFailedEventArgs>
    {
        private readonly int _timeoutMs;
        private readonly ISystemClock _systemClock;
        private Dictionary<string, bool?> _cache;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        public OvernightSwapListener(int timeoutSec,
            ISystemClock systemClock)
        {
            _timeoutMs = timeoutSec * 1000;
            _systemClock = systemClock;
        }
        
        public async Task TrackCharging(string operationId, List<string> operationIds, IEventPublisher publisher)
        {
            if (!operationIds.Any())
            {
                publisher.PublishEvent(new OvernightSwapsChargedEvent(
                    operationId: operationId,
                    creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                    total: 0,
                    failed: 0
                ));
                return;
            }
            
            await _semaphoreSlim.WaitAsync();
            
            _cache = operationIds.ToDictionary(x => x, x => (bool?)null);
            _cancellationTokenSource = new CancellationTokenSource(_timeoutMs);

            try
            {
                await Task.Delay(_timeoutMs, _cancellationTokenSource.Token);
            }
            catch
            {
                // cancellation ignored
            }

            publisher.PublishEvent(new OvernightSwapsChargedEvent(
                operationId: operationId,
                creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                total: _cache.Count,
                failed: _cache.Values.Count(x => x != true)
            ));
            
            _cache = new Dictionary<string, bool?>();

            _semaphoreSlim.Release();
        }

        public void ConsumeEvent(object sender, OvernightSwapChargedEventArgs ea)
        {
            Handler(ea.OperationId, true);
        }

        public void ConsumeEvent(object sender, OvernightSwapChargeFailedEventArgs ea)
        {
            Handler(ea.OperationId, false);
        }

        private void Handler(string operationId, bool type)
        {
            if(!_cache.ContainsKey(operationId))
                return;
            
            _cache[operationId] = type;
            
            if (_cache.All(x => x.Value.HasValue))
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public int ConsumerRank => 100;
    }
}
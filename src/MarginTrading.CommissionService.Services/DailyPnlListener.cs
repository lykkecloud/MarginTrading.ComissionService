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
    public class DailyPnlListener : IDailyPnlListener,
        IEventConsumer<DailyPnlChargedEventArgs>
    {
        private readonly int _timeoutMs;
        private readonly ISystemClock _systemClock;
        private Dictionary<string, bool> _cache;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        public DailyPnlListener(int timeoutSec,
            ISystemClock systemClock)
        {
            _timeoutMs = timeoutSec * 1000;
            _systemClock = systemClock;
        }
        
        public async Task TrackCharging(string operationId, IEnumerable<string> operationIds, IEventPublisher publisher)
        {
            await _semaphoreSlim.WaitAsync();
            
            _cache = operationIds.ToDictionary(x => x, x => false);
            _cancellationTokenSource = new CancellationTokenSource(_timeoutMs);

            try
            {
                await Task.Delay(_timeoutMs, _cancellationTokenSource.Token);
            }
            catch
            {
                // cancellation ignored
            }

            publisher.PublishEvent(new DailyPnlsChargedEvent(
                operationId: operationId,
                creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                total: _cache.Count,
                failed: _cache.Values.Count(x => !x)
            ));
            
            _cache = new Dictionary<string, bool>();

            _semaphoreSlim.Release();
        }

        public void ConsumeEvent(object sender, DailyPnlChargedEventArgs ea)
        {
            if(!_cache.ContainsKey(ea.OperationId))
                return;

            _cache[ea.OperationId] = true;

            if (_cache.All(x => x.Value))
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public int ConsumerRank => 100;
    }
}
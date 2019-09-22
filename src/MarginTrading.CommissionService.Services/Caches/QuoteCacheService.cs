// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using MarginTrading.Backend.Contracts;
using MarginTrading.Backend.Contracts.Prices;
using MarginTrading.Backend.Contracts.Snow.Prices;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Exceptions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class QuoteCacheService : IQuoteCacheService
    {
        private readonly ILog _log;
        private readonly IPricesApi _pricesApi;

        private Dictionary<string, InstrumentBidAskPair> _cache = new Dictionary<string, InstrumentBidAskPair>();
        
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        public QuoteCacheService(ILog log,
            IPricesApi pricesApi)
        {
            _log = log;
            _pricesApi = pricesApi;
        }

        public void Start()
        {
            _log.WriteInfo(nameof(QuoteCacheService), nameof(Start), "Quote cache init started.");

            var quotes = _pricesApi.GetBestAsync(new InitPricesBackendRequest()).GetAwaiter().GetResult();
            
            _log.WriteInfo(nameof(QuoteCacheService), nameof(Start), 
                $"{quotes.Count} quotes read the Trading Core.");

            _cache = quotes.ToDictionary(q => q.Key, q => Map(q.Value));
        }

        public decimal GetQuote(string instrument, OrderDirection orderDirection)
        {
            var quote = GetBidAskPair(instrument);

            return orderDirection == OrderDirection.Buy ? quote.Ask : quote.Bid;
        }

        public InstrumentBidAskPair GetBidAskPair(string instrument)
        {
            _lockSlim.EnterReadLock();
            try
            {
                if (!_cache.TryGetValue(instrument, out var quote))
                    throw new QuoteNotFoundException(instrument, $"There is no quote for instrument {instrument}");

                return quote;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public Task SetQuote(InstrumentBidAskPair quote)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (_cache.ContainsKey(quote.Instrument))
                {
                    _cache[quote.Instrument] = quote;
                }
                else
                {
                    _cache.Add(quote.Instrument, quote);
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
            
            return Task.CompletedTask;
        }

        private InstrumentBidAskPair Map(BestPriceContract contract)
        {
            return new InstrumentBidAskPair
            {
                Instrument = contract.Id,
                Date = contract.Timestamp,
                Ask = contract.Ask,
                Bid = contract.Bid
            };
        }
    }
}
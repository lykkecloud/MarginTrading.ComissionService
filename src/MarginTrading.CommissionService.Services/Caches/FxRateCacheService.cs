// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using MarginTrading.Backend.Contracts;
using MarginTrading.Backend.Contracts.Prices;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Exceptions;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.OrderbookAggregator.Contracts.Messages;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class FxRateCacheService : IFxRateCacheService
    {
        private readonly ILog _log;
        private readonly IPricesApi _pricesApi;
        private Dictionary<string, InstrumentBidAskPair> _quotes;
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();
        

        public FxRateCacheService(ILog log, IPricesApi pricesApi)
        {
            _log = log;
            _pricesApi = pricesApi;
            _quotes = new Dictionary<string, InstrumentBidAskPair>();
        }

        public InstrumentBidAskPair GetQuote(string instrument)
        {
            _lockSlim.EnterReadLock();
            try
            {
                if (!_quotes.TryGetValue(instrument, out var quote))
                    throw new FxRateNotFoundException(instrument, $"There is no fx rate for instrument {instrument}");

                return quote;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public Task SetQuote(ExternalExchangeOrderbookMessage quote)
        {
            var bidAskPair = CreatePair(quote);
            SetQuote(bidAskPair);
            
            return Task.CompletedTask;
        }

        public void SetQuote(InstrumentBidAskPair bidAskPair)
        {
            _lockSlim.EnterWriteLock();
            try
            {

                if (bidAskPair == null)
                {
                    return;
                }

                if (_quotes.ContainsKey(bidAskPair.Instrument))
                {
                    _quotes[bidAskPair.Instrument] = bidAskPair;
                }
                else
                {
                    _quotes.Add(bidAskPair.Instrument, bidAskPair);
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
        
        private InstrumentBidAskPair CreatePair(ExternalExchangeOrderbookMessage message)
        {
            if (!ValidateOrderbook(message))
            {
                return null;
            }
            
            var ask = GetBestPrice(true, message.Asks);
            var bid = GetBestPrice(false, message.Bids);

            return ask == null || bid == null
                ? null
                : new InstrumentBidAskPair
                {
                    Instrument = message.AssetPairId,
                    Date = message.Timestamp,
                    Ask = ask.Value,
                    Bid = bid.Value
                };
        }
        
        private decimal? GetBestPrice(bool isBuy, IReadOnlyCollection<VolumePrice> prices)
        {
            if (!prices.Any())
                return null;
            return isBuy
                ? prices.Min(x => x.Price)
                : prices.Max(x => x.Price);
        }
        
        private bool ValidateOrderbook(ExternalExchangeOrderbookMessage orderbook)
        {
            try
            {
                orderbook.AssetPairId.RequiredNotNullOrWhiteSpace("orderbook.AssetPairId");
                orderbook.ExchangeName.RequiredNotNullOrWhiteSpace("orderbook.ExchangeName");
                orderbook.RequiredNotNull(nameof(orderbook));
                
                orderbook.Bids.RequiredNotNullOrEmpty("orderbook.Bids");
                orderbook.Bids.RemoveAll(e => e == null || e.Price <= 0 || e.Volume == 0);
                orderbook.Bids.RequiredNotNullOrEmptyEnumerable("orderbook.Bids");
                
                orderbook.Asks.RequiredNotNullOrEmpty("orderbook.Asks");
                orderbook.Asks.RemoveAll(e => e == null || e.Price <= 0 || e.Volume == 0);
                orderbook.Asks.RequiredNotNullOrEmptyEnumerable("orderbook.Asks");

                return true;
            }
            catch (Exception e)
            {
                _log.WriteError(nameof(ExternalExchangeOrderbookMessage), orderbook.ToJson(), e);
                return false;
            }
        }
        
        public void Start()
        {
            _quotes =
                _pricesApi.GetBestFxAsync(new InitPricesBackendRequest()).GetAwaiter().GetResult()
                    ?.ToDictionary(d => d.Key,
                        d => new InstrumentBidAskPair
                            {Instrument = d.Value.Id, Ask = d.Value.Ask, Bid = d.Value.Bid, Date = d.Value.Timestamp})
                ?? new Dictionary<string, InstrumentBidAskPair>();
        }
    }
}
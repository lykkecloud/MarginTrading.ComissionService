// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.OrderbookAggregator.Contracts.Messages;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IFxRateCacheService
    {
        InstrumentBidAskPair GetQuote(string instrument);
        Task SetQuote(ExternalExchangeOrderbookMessage quote);
    }
}
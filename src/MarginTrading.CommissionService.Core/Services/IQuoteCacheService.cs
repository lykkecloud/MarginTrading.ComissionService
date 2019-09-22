// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IQuoteCacheService
    {
        decimal GetQuote(string instrument, OrderDirection orderDirection);
        InstrumentBidAskPair GetBidAskPair(string instrument);
        Task SetQuote(InstrumentBidAskPair quote);
    }
}
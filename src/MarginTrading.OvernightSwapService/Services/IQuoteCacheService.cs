using System.Collections.Generic;
using MarginTrading.OvernightSwapService.Models;

namespace MarginTrading.OvernightSwapService.Services
{
    public interface IQuoteCacheService
    {
        InstrumentBidAskPair GetQuote(string instrument);
        Dictionary<string, InstrumentBidAskPair> GetAllQuotes();
        bool TryGetQuoteById(string instrument, out InstrumentBidAskPair result); 
        void RemoveQuote(string assetPair);
    }
}
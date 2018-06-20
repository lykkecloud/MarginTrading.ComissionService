using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class BestPriceChangeEventArgs
    {
        public BestPriceChangeEventArgs(InstrumentBidAskPair pair)
        {
            BidAskPair = pair ?? throw new ArgumentNullException(nameof(pair));
        }

        public InstrumentBidAskPair BidAskPair { get; }
    }
}
using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class InstrumentBidAskPair
    {
        public string Instrument { get; set; }
        public DateTime Date { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }
}
using System;

namespace MarginTrading.OvernightSwapService.Models
{
    public class InstrumentBidAskPair
    {
        public string Instrument { get; set; }
        public DateTime Date { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }
}
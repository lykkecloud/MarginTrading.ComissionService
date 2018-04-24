using System;

namespace MarginTrading.OvernightSwapService.Models.Exceptions
{
    public class AssetPairNotFoundException : Exception
    {
        public string InstrumentId { get; private set; }

        public AssetPairNotFoundException(string instrumentId, string message):base(message)
        {
            InstrumentId = instrumentId;
        }
    }
}
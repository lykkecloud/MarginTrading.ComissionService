using System;
using JetBrains.Annotations;
using MarginTrading.OvernightSwapService.Infrastructure.Implementation;
using MarginTrading.OvernightSwapService.Models.Abstractions;

namespace MarginTrading.OvernightSwapService.Models
{
    public class AssetPair : IAssetPair
    {
        public AssetPair(string id, string name, string baseAssetId,
            string quoteAssetId, int accuracy, string legalEntity,
            [CanBeNull] string basePairId, MatchingEngineMode matchingEngineMode, decimal stpMultiplierMarkupBid,
            decimal stpMultiplierMarkupAsk)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BaseAssetId = baseAssetId ?? throw new ArgumentNullException(nameof(baseAssetId));
            QuoteAssetId = quoteAssetId ?? throw new ArgumentNullException(nameof(quoteAssetId));
            Accuracy = accuracy;
            
            LegalEntity = legalEntity.RequiredNotNullOrWhiteSpace(nameof(legalEntity));
            BasePairId = basePairId;
            MatchingEngineMode = matchingEngineMode.RequiredEnum(nameof(matchingEngineMode));
            StpMultiplierMarkupBid = stpMultiplierMarkupBid.RequiredGreaterThan(0, nameof(stpMultiplierMarkupBid));
            StpMultiplierMarkupAsk = stpMultiplierMarkupAsk.RequiredGreaterThan(0, nameof(stpMultiplierMarkupAsk));
        }

        public string Id { get; }
        public string Name { get; }
        public string BaseAssetId { get; }
        public string QuoteAssetId { get; }
        public int Accuracy { get; }
        
        public string LegalEntity { get; }
        public string BasePairId { get; }
        public MatchingEngineMode MatchingEngineMode { get; }
        public decimal StpMultiplierMarkupBid { get; }
        public decimal StpMultiplierMarkupAsk { get; }
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IAssetPair
    {
        /// <summary>
        /// Instrument id
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Instrument display name
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Base asset id
        /// </summary>
        string BaseAssetId { get; }
        
        /// <summary>
        /// Quoting asset id
        /// </summary>
        string QuoteAssetId { get; }
        
        /// <summary>
        /// Instrument accuracy in decimal digits count
        /// </summary>
        int Accuracy { get; }

        /// <summary>
        /// Id of legal entity
        /// </summary>
        string LegalEntity { get; }

        /// <summary>
        /// Base pair id (ex. BTCUSD for id BTCUSD.cy)
        /// </summary>
        [CanBeNull]
        string BasePairId { get; }

        /// <summary>
        /// How should this asset pair be traded
        /// </summary>
        MatchingEngineMode MatchingEngineMode { get; }

        /// <summary>
        /// Markup for bid for stp mode. 1 results in no changes.
        /// </summary>
        /// <remarks>
        /// You cannot specify a value lower or equal to 0 to ensure positive resulting values.
        /// </remarks>
        decimal StpMultiplierMarkupBid { get; }

        /// <summary>
        /// Markup for ask for stp mode. 1 results in no changes.
        /// </summary>
        /// <remarks>
        /// You cannot specify a value lower or equal to 0 to ensure positive resulting values.
        /// </remarks>
        decimal StpMultiplierMarkupAsk { get; }
        
        /// <summary>
        /// Market
        /// </summary>
        string MarketId { get; }
    }
}
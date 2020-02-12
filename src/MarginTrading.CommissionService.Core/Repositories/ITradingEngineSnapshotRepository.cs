// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.Backend.Contracts.Positions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface ITradingEngineSnapshotRepository
    {
        /// <summary>
        /// Get positions list from the Trading Engine snapshot.
        /// </summary>
        /// <param name="tradingDay"></param>
        /// <returns>Null in case if there was no snapshot for the <paramref name="tradingDay"/></returns>
        Task<List<OpenPositionContract>> GetPositionsAsync(DateTime tradingDay);
    }
}
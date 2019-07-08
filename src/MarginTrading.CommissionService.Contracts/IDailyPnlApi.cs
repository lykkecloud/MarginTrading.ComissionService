// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Refit;

namespace Lykke.MarginTrading.CommissionService.Contracts
{
    /// <summary>
    /// Api for launching daily pnl process. FOR TESTING ONLY
    /// </summary>
    [PublicAPI]
    public interface IDailyPnlApi
    {
        /// <summary>
        /// Starts overnight swap process
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [Post("/api/daily-pnl/start")]
        Task StartDailyPnlProcess([NotNull] string operationId, DateTime tradingDay);
    }
}
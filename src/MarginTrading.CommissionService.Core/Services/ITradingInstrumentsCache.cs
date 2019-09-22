// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ITradingInstrumentsCache
    {
        TradingInstrument Get(string tradingConditionId, string instrument);

        void InitCache(IEnumerable<TradingInstrument> tradingInstruments);

        void Update(TradingInstrument tradingInstrument);

        void Remove(string tradingConditionId, string instrument);
    }
}
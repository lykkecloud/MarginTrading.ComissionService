// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICfdCalculatorService
    {

        decimal GetFxRateForAssetPair(string accountAssetId, string instrument, string legalEntity);

        decimal GetFxRate(string asset1, string asset2, string legalEntity);

    }
}
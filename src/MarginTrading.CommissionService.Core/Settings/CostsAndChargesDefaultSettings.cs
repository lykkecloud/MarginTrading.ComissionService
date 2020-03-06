// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings
{
    public class CostsAndChargesDefaultSettings
    {
        public string LegalEntity { get; set; } = "Default";

        public string[] BaseAssetIds { get; set; } = {"EUR"};
    }
}
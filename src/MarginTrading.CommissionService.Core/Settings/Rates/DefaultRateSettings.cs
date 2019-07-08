// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings.Rates
{
    public class DefaultRateSettings
    {
        public DefaultOrderExecutionSettings DefaultOrderExecutionSettings { get; set; }
        public DefaultOnBehalfSettings DefaultOnBehalfSettings { get; set; }
        public DefaultOvernightSwapSettings DefaultOvernightSwapSettings { get; set; }
    }
}
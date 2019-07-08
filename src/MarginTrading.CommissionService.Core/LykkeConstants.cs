// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core
{
    public class LykkeConstants
    {
        public static string StateBlobContainer = "state";
        public static string RateSettingsBlobContainer = "rate-settings";

        //same keys used for Redis & blob repo
        public static string OrderExecutionKey = "OrderExecution";
        public static string OnBehalfKey = "OnBehalf";
        public static string OvernightSwapKey = "OvernightSwap";
        
        public static string AccountsKey = "Accounts";
    }
}
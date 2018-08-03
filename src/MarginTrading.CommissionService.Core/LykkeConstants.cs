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
    }
}
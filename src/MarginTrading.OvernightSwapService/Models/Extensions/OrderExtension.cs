using MarginTrading.OvernightSwapService.Models.Abstractions;

namespace MarginTrading.OvernightSwapService.Models.Extensions
{
    public static class OrderExtension
    {
        public static OrderDirection GetOrderType(this IOrder order)
        {
            return order.Volume >= 0 ? OrderDirection.Buy : OrderDirection.Sell;
        }
    }
}
using System.Collections.Generic;
using MarginTrading.OvernightSwapService.Models;

namespace MarginTrading.OvernightSwapService.Services
{
    public interface IOrderReceiveService
    {
        IEnumerable<Order> GetActive();
    }
}
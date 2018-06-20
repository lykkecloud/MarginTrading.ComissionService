using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IOrderReceiveService
    {
        IEnumerable<Order> GetActive();
    }
}
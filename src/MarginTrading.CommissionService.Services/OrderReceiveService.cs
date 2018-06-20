using System;
using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class OrderReceiveService : IOrderReceiveService
    {
        private readonly IConvertService _convertService;

        public OrderReceiveService(
            IConvertService convertService)
        {
            _convertService = convertService;
        }
        
        //TODO external impl must be provided.. now we read from current MT datareader
        public IEnumerable<Order> GetActive()
        {
            throw new NotImplementedException();
            /*var orders = _mtDataReaderClient.TradeMonitoringRead.OpenPositions().Result;
            return orders.Select(x => _convertService.Convert<OrderContract, Order>(x));
            */
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.Backend.Contracts.TradeMonitoring;
using MarginTrading.OvernightSwapService.Infrastructure;
using MarginTrading.OvernightSwapService.Models;

namespace MarginTrading.OvernightSwapService.Services.Implementation
{
    public class OrderReceiveService : IOrderReceiveService
    {
        private readonly IMtDataReaderClient _mtDataReaderClient;
        private readonly IConvertService _convertService;

        public OrderReceiveService(
            IMtDataReaderClient mtDataReaderClient,
            IConvertService convertService)
        {
            _mtDataReaderClient = mtDataReaderClient;
            _convertService = convertService;
        }
        
        //TODO external impl must be provided.. now we read from current MT datareader
        public IEnumerable<Order> GetActive()
        {
            var orders = _mtDataReaderClient.TradeMonitoringRead.OpenPositions().Result;
            return orders.Select(x => _convertService.Convert<OrderContract, Order>(x));
        }
    }
}
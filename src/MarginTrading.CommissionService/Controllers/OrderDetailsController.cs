// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Contracts;
using MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Core.Services.OrderDetailsFeature;
using MarginTrading.CommissionService.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.CommissionService.Controllers
{
    [Authorize]
    [Route("api/orderDetails")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class OrderDetailsController : ControllerBase, IOrderDetailsApi
    {
        private readonly IOrderDetailsCalculationService _orderDetailsCalculationService;
        private readonly IOrderDetailsDataSourceBuilder _dataSourceBuilder;
        private readonly IOrderDetailsPdfGenerator _orderDetailsPdfGenerator;

        public OrderDetailsController(IOrderDetailsCalculationService orderDetailsCalculationService,
            IOrderDetailsDataSourceBuilder dataSourceBuilder,
            IOrderDetailsPdfGenerator orderDetailsPdfGenerator)
        {
            _orderDetailsCalculationService = orderDetailsCalculationService;
            _dataSourceBuilder = dataSourceBuilder;
            _orderDetailsPdfGenerator = orderDetailsPdfGenerator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(FileContract), 200)]
        [ProducesResponseType(400)]
        public async Task<FileContract> GenerateOrderDetailsReport([FromQuery] string orderId,
            [FromQuery] string accountId)
        {
            var calculation = await _orderDetailsCalculationService.Calculate(orderId, accountId);
            var dataSource = _dataSourceBuilder.Build(calculation);
            var pdf = _orderDetailsPdfGenerator.GenerateReport(dataSource, new ReportProperties()
            {
                AccountId = calculation.AccountId,
                OrderId = calculation.OrderId,
                IncludeManualConfirmationFooter = calculation.ConfirmedManually,
            });

            return new FileContract()
            {
                Name = $"{accountId}_{orderId}_order_details_{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                Extension = "pdf",
                Content = pdf,
            };
        }
    }
}
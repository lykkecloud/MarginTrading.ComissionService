// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.Services.OrderDetailsFeature;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;

namespace MarginTrading.CommissionService.Tests
{
    public class OrderDetailsGeneratorTests
    {
        private Mock<IHostingEnvironment> _hostingEnvironment = new Mock<IHostingEnvironment>();

        public OrderDetailsGeneratorTests()
        {
            Directory.CreateDirectory("./reports");
        }

        [Test]
        [Ignore("Run manually")]
        public void CreateEnglishOrderDetailsPdf()
        {
            var generator = new OrderDetailsPdfGenerator(new FontProvider("./Fonts/"), _hostingEnvironment.Object);
            var builder = new OrderDetailsDataSourceBuilder(new OrderDetailsEnglishLocalizationService());
            var data = GetData();

            var datasource = builder.Build(data);

            var result = generator.GenerateReport(datasource.Data, datasource.Properties);

            File.WriteAllBytes("reports/test.pdf", result);
        }

        [Test]
        [Ignore("Run manually")]
        public void CreateSpanishOrderDetailsPdf()
        {
            var generator = new OrderDetailsPdfGenerator(new FontProvider("./Fonts/"), _hostingEnvironment.Object);
            var builder = new OrderDetailsDataSourceBuilder(new OrderDetailsSpanishLocalizationService());
            var data = GetData();
            var datasource = builder.Build(data);

            var result = generator.GenerateReport(datasource.Data, datasource.Properties);

            File.WriteAllBytes("reports/sp_test.pdf", result);
        }

        private OrderDetailsData GetData()
        {
            return new OrderDetailsData()
            {
                Instrument = "APPLE_INC",
                OrderDirection = OrderDirection.Buy,
                Origin = OriginatorType.Investor,
                Quantity = 15000,
                OrderId = "Order1234",
                OrderType = OrderType.Limit,
                CreatedTimestamp = DateTime.Now,
                Status = OrderStatus.Executed,
                StopLossPrice = 48.333M,
                ModifiedTimestamp = DateTime.Now,
                TakeProfitPrice = null,
                ExecutedTimestamp = DateTime.Now,
                LimitStopPrice = 48.22M,
                CanceledTimestamp = null,
                ExecutionPrice = 50.1M,
                ValidityTime = null,
                Notional = 500.10M,
                NotionalEUR = 500.10M,
                OrderComment = "My comment",
                ForceOpen = true,
                ExchangeRate = 1,
                Commission = 9.95M,
                ProductCost = 150M,
                TotalCostsAndCharges = 200M,
                ConfirmedManually = false,
                AccountName = "AA001Account"
            };
        }
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
using Lykke.Snow.Common;
using Lykke.Snow.Common.Model;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Repositories;
using Microsoft.Data.SqlClient;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class CommissionHistoryRepository : SqlRepositoryBase, ICommissionHistoryRepository
    {
        private string createTableScript = "commission_history_table.sql";

        private StoredProcedure addCommissionHistory =
            new StoredProcedure("addCommissionHistory", "commission", "dbo", null);

        private StoredProcedure getCommissionHistory =
            new StoredProcedure("getCommissionHistory", "commission", "dbo", null);

        public CommissionHistoryRepository(string connectionString) : base(connectionString)
        {
            Init();
        }

        public CommissionHistoryRepository(string connectionString, int commandTimeout) : base(connectionString,
            commandTimeout)
        {
            Init();
        }

        public async Task AddAsync(CommissionHistory commissionHistory)
        {
            await ExecuteNonQueryAsync(addCommissionHistory, new[]
            {
                new SqlParameter("@orderId", commissionHistory.OrderId),
                new SqlParameter("@commission", commissionHistory.Commission),
                new SqlParameter("@productCost", commissionHistory.ProductCost),
                new SqlParameter("@productCostCalculationData", commissionHistory.ProductCostCalculationData.ToJson()),
            });
        }

        public async Task<CommissionHistory> GetByOrderIdAsync(string orderId)
        {
            return await GetAsync(getCommissionHistory, new[] {new SqlParameter("@orderId", orderId)},
                reader => Map(reader));
        }

        private CommissionHistory Map(SqlDataReader reader)
        {
            var swapRate = reader[nameof(CommissionHistory.Commission)] as string;

            return new CommissionHistory()
            {
                OrderId = reader[nameof(CommissionHistory.OrderId)] as string,
                Commission = reader[nameof(CommissionHistory.Commission)] as decimal?,
                ProductCost = reader[nameof(CommissionHistory.Commission)] as decimal?,
                ProductCostCalculationData = string.IsNullOrEmpty(swapRate) ? null : swapRate.DeserializeJson<ProductCostCalculationData>(),
            };
        }

        private void Init()
        {
            ExecCreateOrAlter(createTableScript);
            ExecCreateOrAlter(addCommissionHistory);
            ExecCreateOrAlter(getCommissionHistory);
        }
    }
}
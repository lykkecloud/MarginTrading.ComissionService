// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Data.Common;
using Lykke.Common.MsSql;
using MarginTrading.CommissionService.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarginTrading.CommissionService.SqlRepositories
{
    public class CommissionDbContext : MsSqlContext
    {
        private const string Schema = "commission";
        
        internal DbSet<KidScenarioEntity> KidScenarios { get; set; }

        public CommissionDbContext() : base(Schema)
        {
        }
        
        public CommissionDbContext(int commandTimeoutSeconds = 30) : base(Schema, commandTimeoutSeconds)
        {
        }

        public CommissionDbContext(string connectionString, bool isTraceEnabled, int commandTimeoutSeconds = 30) : base(Schema, connectionString, isTraceEnabled, commandTimeoutSeconds)
        {
        }

        public CommissionDbContext(DbContextOptions contextOptions) : base(Schema, contextOptions)
        {
        }
        
        public CommissionDbContext(DbConnection dbConnection) : base(Schema, dbConnection)
        {
        }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommissionDbContext).Assembly);
        }
    }
}
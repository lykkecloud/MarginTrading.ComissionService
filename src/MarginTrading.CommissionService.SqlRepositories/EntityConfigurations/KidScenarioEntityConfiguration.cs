// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarginTrading.CommissionService.SqlRepositories.EntityConfigurations
{
    public class KidScenarioEntityConfiguration : IEntityTypeConfiguration<KidScenarioEntity>
    {
        private const int MaxLength = 400;
        private const string DbDecimal = "decimal(18,2)";
        
        public void Configure(EntityTypeBuilder<KidScenarioEntity> builder)
        {
            builder.HasKey(x => x.Isin);
            
            builder.Property(x => x.Isin).HasMaxLength(MaxLength);
            builder.Property(x => x.KidModerateScenario).HasColumnType(DbDecimal);
            builder.Property(x => x.KidModerateScenarioAvreturn).HasColumnType(DbDecimal);
            builder.Property(x => x.Timestamp).IsRequired();
        }
    }
}
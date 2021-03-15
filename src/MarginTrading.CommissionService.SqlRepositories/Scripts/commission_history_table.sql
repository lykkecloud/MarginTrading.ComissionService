IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE [name] = 'CommissionHistory' AND schema_id = schema_id('dbo'))
BEGIN
CREATE TABLE [dbo].[CommissionHistory](
    [OrderId] [nvarchar](50) NOT NULL,
    [Commission] [decimal](24, 12) NOT NULL,
    [ProductCost] [decimal](24, 12) NULL,
    CONSTRAINT [PK_CommissionHistory] PRIMARY KEY CLUSTERED
(
[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1 FROM sys.columns  WHERE object_id = OBJECT_ID(N'[dbo].[CommissionHistory]') and  [name] = 'ProductCostCalculationData')
BEGIN

    ALTER TABLE [dbo].[CommissionHistory]
    ADD [ProductCostCalculationData] [nvarchar](2000) NULL;

END


CREATE OR ALTER PROCEDURE addCommissionHistory
    @orderId  [nvarchar](50),
    @commission [decimal](24, 12),
    @productCostCalculationData [nvarchar](2000)
    AS
BEGIN

insert into dbo.CommissionHistory (orderId, commission, productCost, productCostCalculationData)
values (@orderId, @commission, null, @productCostCalculationData);

END


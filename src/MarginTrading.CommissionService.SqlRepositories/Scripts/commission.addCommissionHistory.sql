CREATE OR ALTER PROCEDURE addCommissionHistory
    @orderId  [nvarchar](50),
    @commission [decimal](24, 12),
    @productCost [decimal](24, 12)
    AS
BEGIN

insert into dbo.CommissionHistory (orderId, commission, productCost)
values (@orderId, @commission, @productCost);

END


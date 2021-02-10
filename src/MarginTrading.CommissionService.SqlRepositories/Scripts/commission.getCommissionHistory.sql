CREATE OR ALTER PROCEDURE getCommissionHistory
    @orderId  [nvarchar](50)
    AS
BEGIN

select * from dbo.CommissionHistory (nolock)
where orderId = @orderId

END

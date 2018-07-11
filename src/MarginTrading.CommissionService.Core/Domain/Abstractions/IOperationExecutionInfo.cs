namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IOperationExecutionInfo<T> where T: class
    {
        string OperationName { get; }
        string Id { get; }

        T Data { get; }
    }
}
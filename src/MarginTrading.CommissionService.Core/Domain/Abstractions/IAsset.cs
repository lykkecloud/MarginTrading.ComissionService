namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IAsset
    {
        string Id { get; }
        string Name { get; }
        int Accuracy { get; }
    }
}
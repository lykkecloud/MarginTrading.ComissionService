namespace MarginTrading.OvernightSwapService.Models.Abstractions
{
    public interface IAsset
    {
        string Id { get; }
        string Name { get; }
        int Accuracy { get; }
    }
}
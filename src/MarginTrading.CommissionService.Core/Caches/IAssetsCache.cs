namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IAssetsCache
    {
        int GetAssetAccuracy(string assetId);
    }
}
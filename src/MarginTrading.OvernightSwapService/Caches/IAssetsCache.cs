namespace MarginTrading.OvernightSwapService.Caches
{
    public interface IAssetsCache
    {
        int GetAssetAccuracy(string assetId);
    }
}
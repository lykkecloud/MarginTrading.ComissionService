using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IAssetsCache
    {
        void Initialize(Dictionary<string, Asset> data);

        int GetAccuracy(string id);
    }
}
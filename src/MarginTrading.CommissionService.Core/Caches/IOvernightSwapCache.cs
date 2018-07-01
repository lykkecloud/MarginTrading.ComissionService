using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IOvernightSwapCache
    {
        bool TryGet(string key, out OvernightSwapCalculation item);
        IReadOnlyList<OvernightSwapCalculation> GetAll();
        bool AddOrReplace(OvernightSwapCalculation item);
        void Remove(OvernightSwapCalculation item);
        void ClearAll();
        void Initialize(IEnumerable<OvernightSwapCalculation> items);
    }
}
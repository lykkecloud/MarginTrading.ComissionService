using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts.Messages;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IAssetPairsManager
    {
        /// <summary>
        /// Initialize asset pairs cache
        /// </summary>
        void InitAssetPairs();

        Task HandleSettingsChanged(SettingsChangedEvent evt);
    }
}
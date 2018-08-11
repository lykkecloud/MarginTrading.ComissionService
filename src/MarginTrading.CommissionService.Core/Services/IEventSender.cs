using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IEventSender
    {
        Task SendRateSettingsChanged(CommissionType type);
    }
}
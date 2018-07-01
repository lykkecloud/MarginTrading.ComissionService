using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface ILogRepository
    {
        Task Insert(ILogEntity log);
    }
}
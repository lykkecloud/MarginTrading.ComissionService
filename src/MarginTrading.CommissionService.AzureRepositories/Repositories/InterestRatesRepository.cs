using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.AzureRepositories.Repositories
{
    public class InterestRatesRepository: IInterestRatesRepository
    {
        private readonly INoSQLTableStorage<InterestRateEntity> _tableStorage;
		
        public InterestRatesRepository(INoSQLTableStorage<InterestRateEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        
        public async Task<IReadOnlyList<IInterestRate>> GetAll()
        {
            return (await _tableStorage.GetDataAsync()).ToList();
        }

        public async Task<IReadOnlyList<IInterestRate>> GetAllLatest()
        {
            return (await _tableStorage.GetDataAsync()).GroupBy(x => x.PartitionKey)
                .Select(x => x.OrderByDescending(z => z.Timestamp).First()).ToList();
        }
    }
}
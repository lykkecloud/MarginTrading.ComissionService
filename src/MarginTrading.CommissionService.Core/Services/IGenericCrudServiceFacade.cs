using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IGenericCrudServiceFacade<T>
        where T: class, IKeyedObject
    {
        [ItemCanBeNull]
        Task<T> Get([NotNull] string key);

        [ItemCanBeNull]
        Task<IReadOnlyList<T>> GetMany(string predicateKey = null);

        Task Replace([NotNull] IReadOnlyList<T> objects);

        Task Delete(IReadOnlyList<T> objects);
    }
}
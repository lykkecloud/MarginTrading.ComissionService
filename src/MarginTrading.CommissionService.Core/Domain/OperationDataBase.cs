using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class OperationDataBase<TState>
        where TState : struct, IConvertible
    {
        public TState State { get; set; }
    }
}
using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class OperationExecutionInfo<T> : IOperationExecutionInfo<T> 
        where T: class
    {
        public string OperationName { get; }
        public string Id { get; }

        public T Data { get; }

        public OperationExecutionInfo([NotNull] string operationName, [NotNull] string id, 
            [NotNull] T data)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}
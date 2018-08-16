using System;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.SqlRepositories.Entities
{
    public class OperationExecutionInfoEntity : IOperationExecutionInfo<object>
    {
        public string OperationName { get; set; }
        
        public string Id { get; set; }
        public DateTime LastModified { get; set; }

        object IOperationExecutionInfo<object>.Data => JsonConvert.DeserializeObject<object>(Data);
        public string Data { get; set; }
        
    }
}
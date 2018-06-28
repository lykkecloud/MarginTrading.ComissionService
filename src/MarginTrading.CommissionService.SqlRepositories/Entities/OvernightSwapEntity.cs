using System;
using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.SqlRepositories.Entities
{
    public class OvernightSwapEntity : IOvernightSwapCalculation
    {
        public string OperationId { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public string Direction { get; set; }
        PositionDirection? IOvernightSwapCalculation.Direction => 
            Enum.TryParse<PositionDirection>(Direction, out var direction) ? direction : (PositionDirection?)null;
        public DateTime Time { get; set; }
        public decimal Volume { get; set; }
        public decimal SwapValue { get; set; }
        public string PositionId { get; set; }
		
        public bool IsSuccess { get; set; }
        public string Exception { get; set; }
        Exception IOvernightSwapCalculation.Exception => JsonConvert.DeserializeObject<Exception>(Exception);
		
        public static OvernightSwapEntity Create(IOvernightSwapCalculation obj)
        {
            return new OvernightSwapEntity
            {
                OperationId = obj.OperationId,
                AccountId = obj.AccountId,
                Instrument = obj.Instrument,
                Direction = obj.Direction?.ToString(),
                Time = obj.Time,
                Volume = obj.Volume,
                SwapValue = obj.SwapValue,
                PositionId = obj.PositionId,
                IsSuccess = obj.IsSuccess,
                Exception = JsonConvert.SerializeObject(obj.Exception)
            };
        }
    }
}
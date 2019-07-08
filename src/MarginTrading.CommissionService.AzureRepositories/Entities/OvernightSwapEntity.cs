// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.AzureRepositories.Entities
{
    public class OvernightSwapEntity : AzureTableEntity, IOvernightSwapCalculation
    {
        public string Id => OvernightSwapCalculation.GetId(OperationId, PositionId);
        
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
        public string Details { get; set; }
        public DateTime TradingDay { get; set; }

        public bool IsSuccess { get; set; }
        public string Exception { get; set; }
        Exception IOvernightSwapCalculation.Exception => JsonConvert.DeserializeObject<Exception>(Exception);
        
        public bool? WasCharged { get; set; }
		
        public static OvernightSwapEntity Create(IOvernightSwapCalculation obj)
        {
            return new OvernightSwapEntity
            {
                PartitionKey = obj.AccountId,
                RowKey = obj.Id,
                
                OperationId = obj.OperationId,
                AccountId = obj.AccountId,
                Instrument = obj.Instrument,
                Direction = obj.Direction?.ToString(),
                Time = obj.Time,
                Volume = obj.Volume,
                SwapValue = obj.SwapValue,
                PositionId = obj.PositionId,
                Details = obj.Details,
                TradingDay = obj.TradingDay,
                IsSuccess = obj.IsSuccess,
                Exception = JsonConvert.SerializeObject(obj.Exception),
                WasCharged = obj.WasCharged,
            };
        }
    }
}
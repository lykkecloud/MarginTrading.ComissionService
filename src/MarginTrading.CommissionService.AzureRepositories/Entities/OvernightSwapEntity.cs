using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.AzureRepositories.Entities
{
    public class OvernightSwapEntity : AzureTableEntity, IOvernightSwap
    {
        public string ClientId { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public string Direction { get; set; }
        OrderDirection? IOvernightSwap.Direction => 
            Enum.TryParse<OrderDirection>(Direction, out var direction) ? direction : (OrderDirection?)null;
        public DateTime Time { get; set; }
        public decimal Volume { get; set; }
        public string OpenOrderIds { get; set; }
        List<string> IOvernightSwap.OpenOrderIds => JsonConvert.DeserializeObject<List<string>>(OpenOrderIds);
        public decimal Value { get; set; }
        public decimal SwapRate { get; set; }
		
        public bool IsSuccess { get; set; }
        public string Exception { get; set; }
        Exception IOvernightSwap.Exception => JsonConvert.DeserializeObject<Exception>(Exception);
		
        public static OvernightSwapEntity Create(IOvernightSwap obj)
        {
            return new OvernightSwapEntity
            {
                PartitionKey = obj.AccountId,
                RowKey = $"{obj.Time:O}",
                ClientId = obj.ClientId,
                AccountId = obj.AccountId,
                Instrument = obj.Instrument,
                Direction = obj.Direction?.ToString(),
                Time = obj.Time,
                Volume = obj.Volume,
                Value = obj.Value,
                SwapRate = obj.SwapRate,
                OpenOrderIds = JsonConvert.SerializeObject(obj.OpenOrderIds),
                IsSuccess = obj.IsSuccess,
                Exception = JsonConvert.SerializeObject(obj.Exception)
            };
        }
    }
}
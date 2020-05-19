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
    public class DailyPnlEntity : AzureTableEntity, IDailyPnlCalculation
    {
        public string Id => DailyPnlCalculation.GetId(OperationId, PositionId);
        
        public string OperationId { get; private set; }
        public string AccountId { get; private set; }
        public string Instrument { get; private set; }
        public DateTime Time { get; private set; }
        public DateTime TradingDay { get; private set; }
        public decimal Volume { get; private set; }
        public decimal FxRate { get; private set; }
        public string PositionId { get; private set; }
        public decimal Pnl { get; private set; }
        public bool IsSuccess { get; private set; }
        public string Exception { get; set; }
        Exception IDailyPnlCalculation.Exception => JsonConvert.DeserializeObject<Exception>(Exception);
        public bool? WasCharged { get; private set; }
        public decimal RawTotalPnl { get; private set; }

        public static DailyPnlEntity Create(IDailyPnlCalculation obj)
        {
            return new DailyPnlEntity
            {
                OperationId = obj.OperationId,
                AccountId = obj.AccountId,
                Instrument = obj.Instrument,
                Time = obj.Time,
                TradingDay = obj.TradingDay,
                Volume = obj.Volume,
                FxRate = obj.FxRate,
                PositionId = obj.PositionId,
                Pnl = obj.Pnl,
                IsSuccess = obj.IsSuccess,
                Exception = JsonConvert.SerializeObject(obj.Exception),
                WasCharged = obj.WasCharged,
                RawTotalPnl = obj.RawTotalPnl
            };
        }
    }
}
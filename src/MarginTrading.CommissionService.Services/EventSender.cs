﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using Lykke.MarginTrading.CommissionService.Contracts.Messages;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.AssetService.Contracts.Messages;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class EventSender : IEventSender
    {
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;
        private readonly IMessageProducer<RateSettingsChangedEvent> _rateSettingsChangedEventProducer;

        public EventSender(
            IRabbitMqService rabbitMqService,
            ILog log,
            ISystemClock systemClock,
            RabbitMqSettings rabbitMqSettings)
        {
            _log = log;
            _systemClock = systemClock;
            
            _rateSettingsChangedEventProducer =
                rabbitMqService.GetProducer(rabbitMqSettings.Publishers.RateSettingsChanged, true,
                    rabbitMqService.GetJsonSerializer<RateSettingsChangedEvent>());
        }

        public async Task SendRateSettingsChanged(CommissionType type)
        {
            await _rateSettingsChangedEventProducer.ProduceAsync(new RateSettingsChangedEvent
            {
                CreatedTimeStamp = _systemClock.UtcNow.UtcDateTime,
                Type = type.ToType<CommissionTypeContract>()
            });
        }
    }
}
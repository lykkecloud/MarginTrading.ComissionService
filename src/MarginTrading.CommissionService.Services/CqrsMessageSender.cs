// Copyright (c) 2019 Lykke Corp.
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
using MarginTrading.SettingsService.Contracts.Enums;
using MarginTrading.SettingsService.Contracts.Messages;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class CqrsMessageSender : ICqrsMessageSender
    {
        public ICqrsEngine _cqrsEngine { get; set; }//property injection
        private readonly CqrsContextNamesSettings _contextNames;

        public CqrsMessageSender(
            ISystemClock systemClock,
            CqrsContextNamesSettings contextNames)
        {
            _contextNames = contextNames;
        }

        public Task SendHandleExecutedOrderInternalCommand(HandleOrderExecInternalCommand command)
        {
            _cqrsEngine.SendCommand(command, 
                _contextNames.CommissionService, 
                _contextNames.CommissionService);
            
            return Task.CompletedTask;
        }

        public Task SendHandleOnBehalfInternalCommand(HandleOnBehalfInternalCommand command)
        {
            _cqrsEngine.SendCommand(command,
                _contextNames.CommissionService, 
                _contextNames.CommissionService);
            
            return Task.CompletedTask;
        }

        public void PublishEvent<T>(T ev, string boundedContext = null)
        {
            try
            {
                _cqrsEngine.PublishEvent(ev, boundedContext ?? _contextNames.CommissionService);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
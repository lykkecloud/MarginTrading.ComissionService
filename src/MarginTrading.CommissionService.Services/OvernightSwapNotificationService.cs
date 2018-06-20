using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Log;
using Lykke.Common;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class OvernightSwapNotificationService : IOvernightSwapNotificationService
    {
        private readonly IAssetPairsCache _assetPairsCache;
        private readonly IEmailService _emailService;

        private readonly IOvernightSwapHistoryRepository _overnightSwapHistoryRepository;
        
        private readonly IThreadSwitcher _threadSwitcher;
        
        private readonly ILog _log;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        public OvernightSwapNotificationService(
            IAssetPairsCache assetPairsCache,
            IEmailService emailService,
            
            IOvernightSwapHistoryRepository overnightSwapHistoryRepository,
            
            IThreadSwitcher threadSwitcher,
            
            ILog log)
        {
            _assetPairsCache = assetPairsCache;
            _emailService = emailService;

            _overnightSwapHistoryRepository = overnightSwapHistoryRepository;
            
            _threadSwitcher = threadSwitcher;

            _log = log;
        }
        
        public void PerformEmailNotification(DateTime calculationTime)
        {
            _threadSwitcher.SwitchThread(async () =>
            {
                await _semaphore.WaitAsync();

                try
                {
                    var processedCalculations = (await _overnightSwapHistoryRepository.GetAsync(calculationTime, null))
                        .Where(x => x.IsSuccess && x.Time >= calculationTime)
                        .ToList();
                    
                    //TODO remake to use a single calculation for each order ?? discuss with Vera
                    var notifications = processedCalculations
                        .GroupBy(x => x.ClientId)
                        .Select(c => new OvernightSwapNotification
                            {
                                CliendId = c.Key,
                                CalculationsByAccount = c.GroupBy(a => a.AccountId)
                                    .Select(a =>
                                    {
                                        return new OvernightSwapNotification.AccountCalculations()
                                        {
                                            AccountId = a.Key,
                                            AccountCurrency = "Base asset id",//TODO get base asset id from order
                                            Calculations = a.Select(calc =>
                                            {
                                                var instrumentName = _assetPairsCache.GetAssetPairByIdOrDefault(calc.Instrument)?.Name 
                                                                     ?? calc.Instrument;
                                                return new OvernightSwapNotification.SingleCalculation
                                                {
                                                    Instrument =  instrumentName,
                                                    Direction = calc.Direction == OrderDirection.Buy ? "Long" : "Short",
                                                    Volume = calc.Volume,
                                                    SwapRate = calc.SwapRate,
                                                    Cost = calc.Value,
                                                    PositionIds = calc.OpenOrderIds,
                                                };
                                            }).ToList()
                                        };
                                    }).ToList()
                            }
                        );

                    var clientsWithIncorrectMail = new List<string>();
                    var clientsSentEmails = new List<string>();
                    foreach (var notification in notifications)
                    {
                        try
                        {
                            //TODO batch such requests / cache
                            var clientEmail = "";//(await _clientAccountClient.GetByIdAsync(notification.CliendId))?.Email;
                            if (string.IsNullOrEmpty(clientEmail))
                            {
                                clientsWithIncorrectMail.Add(notification.CliendId);
                                continue;
                            }

                            await _emailService.SendOvernightSwapEmailAsync(clientEmail, notification);
                            clientsSentEmails.Add(notification.CliendId);
                        }
                        catch (Exception e)
                        {
                            await _log.WriteErrorAsync(nameof(OvernightSwapNotificationService),
                                nameof(PerformEmailNotification), e, DateTime.UtcNow);
                        }
                    }

                    if (clientsWithIncorrectMail.Any())
                        await _log.WriteWarningAsync(nameof(OvernightSwapNotificationService), nameof(PerformEmailNotification),
                            $"Emails of some clients are incorrect: {string.Join(", ", clientsWithIncorrectMail)}.", DateTime.UtcNow);
                    if (clientsSentEmails.Any())
                        await _log.WriteInfoAsync(nameof(OvernightSwapNotificationService), nameof(PerformEmailNotification),
                            $"Emails sent to: {string.Join(", ", clientsSentEmails)}.", DateTime.UtcNow);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }
    }
}
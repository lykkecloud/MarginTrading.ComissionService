using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.CommissionService.Core.Settings
{
    [UsedImplicitly]
    public class CqrsSettings
     {
         [AmqpCheck]
         public string ConnectionString { get; set; }
 
         public TimeSpan RetryDelay { get; set; }
 
         [Optional, CanBeNull]
         public string EnvironmentName { get; set; }

         [Optional]
         public uint CommandsHandlersThreadCount { get; set; } = 8;

         [Optional]
         public uint CommandsHandlersQueueCapacity { get; set; } = 1024;
 
         [Optional]
         public CqrsContextNamesSettings ContextNames { get; set; } = new CqrsContextNamesSettings();
     }
 }
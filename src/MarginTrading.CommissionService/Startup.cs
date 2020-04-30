// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.AzureQueueIntegration;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Cqrs;
using Lykke.Logs;
using Lykke.Logs.MsSql;
using Lykke.Logs.MsSql.Repositories;
using Lykke.Logs.Serilog;
using Lykke.MarginTrading.CommissionService.Contracts.Api;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;
using Lykke.Snow.Common.Startup;
using Lykke.Snow.Common.Startup.ApiKey;
using Lykke.Snow.Common.Startup.ApiKey.Validator;
using Lykke.Snow.Common.Startup.Hosting;
using Lykke.Snow.Common.Startup.Log;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Infrastructure;
using MarginTrading.CommissionService.Modules;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.Services.Caches;
using MarginTrading.CommissionService.SqlRepositories.Repositories;
using MarginTrading.CommissionService.Workflow;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.SettingsService.Contracts.Messages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.CommissionService
{
    public class Startup
    {
        public static string ServiceName { get; } = PlatformServices.Default.Application.ApplicationName;
        private IReloadingManager<AppSettings> _mtSettingsManager;
        private IHostingEnvironment Environment { get; }
        private ILifetimeScope ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        [CanBeNull] private ILog Log { get; set; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddSerilogJson(env)
                .AddEnvironmentVariables()
                .Build();
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddControllers()
                    .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    });

                _mtSettingsManager = Configuration.LoadSettings<AppSettings>(
                        throwExceptionOnCheckError: !Configuration.NotThrowExceptionsOnServiceValidation())
                    .Nested(s =>
                    {
                        s.CommissionService.InstanceId = Configuration.InstanceId() ?? Guid.NewGuid().ToString("N");

                        return s;
                    });
                
                services.AddApiKeyAuth(_mtSettingsManager.CurrentValue.CommissionServiceClient);
                
                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", ServiceName + " API");

                    if (!string.IsNullOrWhiteSpace(_mtSettingsManager.CurrentValue.CommissionServiceClient?.ApiKey))
                    {
                        options.AddApiKeyAwareness();
                    }
                });

                Log = CreateLog(Configuration, services, _mtSettingsManager);

                services.AddSingleton<ILoggerFactory>(x => new WebHostLoggerFactory(Log));
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new CommissionServiceModule(_mtSettingsManager, Log));
            builder.RegisterModule(new CommissionServiceExternalModule(_mtSettingsManager));
            builder.RegisterModule(new CqrsModule(_mtSettingsManager.CurrentValue.CommissionService.Cqrs, Log));
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                ApplicationContainer = app.ApplicationServices.GetAutofacRoot();

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }

#if DEBUG
                app.UseLykkeMiddleware(ServiceName, ex => ex.ToString());
#else
                app.UseLykkeMiddleware(ServiceName, ex => new ErrorResponse {ErrorMessage = "Technical problem", Details
 = ex.Message});
#endif

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                app.UseSwagger();
                app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/v1/swagger.json", "Main Swagger"));

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopping.Register(() => StopApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        private Task StartApplication()
        {
            try
            {
                //TODO that's only to get swap commission values - EOD keeper will provide interest rates

                // NOTE: Service not yet receives and processes requests here
                var settings = ApplicationContainer.Resolve<CommissionServiceSettings>();
                var rabbitMqService = ApplicationContainer.Resolve<IRabbitMqService>();
                var fxRateCacheService = ApplicationContainer.Resolve<IFxRateCacheService>();
                var quotesCacheService = ApplicationContainer.Resolve<IQuoteCacheService>();
                var executedOrdersHandlingService = ApplicationContainer.Resolve<IExecutedOrdersHandlingService>();
                var assetPairManager = ApplicationContainer.Resolve<ISettingsManager>();
                var accountMarginEventsProjection = ApplicationContainer.Resolve<AccountMarginEventsProjection>();
                var cqrsEngine = ApplicationContainer.Resolve<ICqrsEngine>();

                rabbitMqService.Subscribe(settings.RabbitMq.Consumers.FxRateRabbitMqSettings, false,
                    fxRateCacheService.SetQuote,
                    rabbitMqService.GetMsgPackDeserializer<ExternalExchangeOrderbookMessage>(), settings.InstanceId);

                rabbitMqService.Subscribe(settings.RabbitMq.Consumers.QuotesRabbitMqSettings, false,
                    quote => quotesCacheService.SetQuote(quote),
                    rabbitMqService.GetJsonDeserializer<InstrumentBidAskPair>(), settings.InstanceId);

                rabbitMqService.Subscribe(settings.RabbitMq.Consumers.OrderExecutedSettings, true,
                    executedOrdersHandlingService.Handle, rabbitMqService.GetJsonDeserializer<OrderHistoryEvent>());

                rabbitMqService.Subscribe(settings.RabbitMq.Consumers.SettingsChanged, false,
                    arg => assetPairManager.HandleSettingsChanged(arg),
                    rabbitMqService.GetJsonDeserializer<SettingsChangedEvent>(), settings.InstanceId);

                rabbitMqService.Subscribe(settings.RabbitMq.Consumers.AccountMarginEvents, true,
                    arg => accountMarginEventsProjection.Handle(arg),
                    rabbitMqService.GetJsonDeserializer<MarginEventMessage>(), settings.InstanceId);

                cqrsEngine.StartSubscribers();
                cqrsEngine.StartProcesses();

                Program.AppHost.WriteLogs(Environment, LogLocator.CommonLog);

                Log?.WriteMonitorAsync("", "", "Started").Wait();
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex).Wait();
                throw;
            }

            return Task.CompletedTask;
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can receive and process requests here, so take care about it if you add logic here.
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                }

                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't receive and process requests here, so you can destroy all resources

                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }

                throw;
            }
        }

        private static ILog CreateLog(IConfiguration configuration, IServiceCollection services,
            IReloadingManager<AppSettings> settings)
        {
            const string requestsLogName = "CommissionServiceRequestsLog";
            const string logName = "CommissionServiceLog";
            var consoleLogger = new LogToConsole();

            #region Logs settings validation

            if (!settings.CurrentValue.CommissionService.UseSerilog
                && string.IsNullOrWhiteSpace(settings.CurrentValue.CommissionService.Db.LogsConnString))
            {
                throw new Exception("Either UseSerilog must be true or LogsConnString must be set");
            }

            #endregion Logs settings validation

            if (settings.CurrentValue.CommissionService.UseSerilog)
            {
                var serilogLogger = new SerilogLogger(typeof(Startup).Assembly, configuration);

                LogLocator.RequestsLog = LogLocator.CommonLog = serilogLogger;

                return serilogLogger;
            }

            if (settings.CurrentValue.CommissionService.Db.StorageMode == StorageMode.SqlServer)
            {
                LogLocator.CommonLog = new AggregateLogger(
                    new LogToSql(new SqlLogRepository(logName,
                        settings.CurrentValue.CommissionService.Db.LogsConnString)),
                    new LogToConsole());

                LogLocator.RequestsLog = new AggregateLogger(
                    new LogToSql(new SqlLogRepository(requestsLogName,
                        settings.CurrentValue.CommissionService.Db.LogsConnString)),
                    new LogToConsole());

                return LogLocator.CommonLog;
            }

            if (settings.CurrentValue.CommissionService.Db.StorageMode != StorageMode.Azure)
            {
                throw new Exception("Wrong config! Logging must be set either to Serilog, SqlServer or Azure.");
            }

            #region Azure logging

            LogLocator.RequestsLog = services.UseLogToAzureStorage(settings.Nested(s =>
                    s.CommissionService.Db.LogsConnString),
                null, requestsLogName, consoleLogger);

            LogLocator.CommonLog = services.UseLogToAzureStorage(settings.Nested(s =>
                    s.CommissionService.Db.LogsConnString),
                null, logName, consoleLogger);

            return LogLocator.CommonLog;

            #endregion Azure logging
        }
    }
}
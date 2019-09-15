// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Logs.MsSql;
using Lykke.Logs.MsSql.Repositories;
using Lykke.Logs.Serilog;
using Lykke.MarginTrading.CommissionService.Contracts.Api;
using Lykke.SettingsReader;
using Lykke.Snow.Common.Startup;
using Lykke.Snow.Common.Startup.ApiKey;
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
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.SettingsService.Contracts.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MarginTrading.CommissionService
{
    public class Startup
    {
        public static string ServiceName { get; } = PlatformServices.Default.Application.ApplicationName;

        private IHostingEnvironment Environment { get; }
        private IContainer ApplicationContainer { get; set; }
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

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    });
                
                var appSettings = Configuration.LoadSettings<AppSettings>(
                        throwExceptionOnCheckError: !Configuration.NotThrowExceptionsOnServiceValidation())
                    .Nested(s =>
                    {
                        s.CommissionService.InstanceId = Configuration.InstanceId() ?? Guid.NewGuid().ToString("N");

                        return s;
                    });
                
                services.AddApiKeyAuth(appSettings.CurrentValue.CommissionServiceClient);

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", ServiceName + " API");
                    options.OperationFilter<CustomOperationIdOperationFilter>();
                    if (!string.IsNullOrWhiteSpace(appSettings.CurrentValue.CommissionServiceClient?.ApiKey))
                    {
                        options.OperationFilter<ApiKeyHeaderOperationFilter>();
                    }
                });

                var builder = new ContainerBuilder();

                Log = CreateLog(Configuration, services, appSettings);

                builder.RegisterModule(new CommissionServiceModule(appSettings, Log));
                builder.RegisterModule(new CommissionServiceExternalModule(appSettings));
                builder.RegisterModule(new CqrsModule(appSettings.CurrentValue.CommissionService.Cqrs, Log));

                builder.Populate(services);

                ApplicationContainer = builder.Build();
                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
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
                app.UseLykkeMiddleware(ServiceName, ex => new ErrorResponse {ErrorMessage = "Technical problem", Details = ex.Message});
#endif
                
                app.UseAuthentication();
                app.UseMvc();
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
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            if (settings.CurrentValue.CommissionService.UseSerilog)
            {
                aggregateLogger.AddLog(new SerilogLogger(typeof(Startup).Assembly, configuration));
            }
            else if (settings.CurrentValue.CommissionService.Db.StorageMode == StorageMode.SqlServer)
            {
                aggregateLogger.AddLog(new LogToSql(new SqlLogRepository("CommissionServiceLog",
                    settings.CurrentValue.CommissionService.Db.LogsConnString)));
            }
            else if (settings.CurrentValue.CommissionService.Db.StorageMode == StorageMode.Azure)
            {
                aggregateLogger.AddLog(services.UseLogToAzureStorage(settings.Nested(s => s.CommissionService.Db.LogsConnString),
                    null, "CommissionServiceLog", consoleLogger));
            }

            LogLocator.Log = aggregateLogger;
            
            return aggregateLogger;
        }
    }
}
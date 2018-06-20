using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using FluentScheduler;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.OvernightSwapService.Infrastructure.Implementation;
using MarginTrading.OvernightSwapService.Modules;
using MarginTrading.OvernightSwapService.Scheduling;
using MarginTrading.OvernightSwapService.Services;
using MarginTrading.OvernightSwapService.Services.Implementation;
using MarginTrading.OvernightSwapService.Settings;
using MarginTrading.OvernightSwapService.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MarginTrading.OvernightSwapService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"SettingsUrl", Path.Combine(env.ContentRootPath, "appsettings.dev.json")}
                })
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }

        public static string ServiceName { get; } = PlatformServices.Default.Application.ApplicationName;

        private IHostingEnvironment Environment { get; }
        private IContainer ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        [CanBeNull] private ILog Log { get; set; }

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

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", ServiceName + " API");
                    options.OperationFilter<CustomOperationIdOperationFilter>();
                });

                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>();
                Log = CreateLog(services, appSettings);

                builder.RegisterModule(new OvernightSwapServiceModule(appSettings, Log));
                builder.RegisterModule(new OvernightSwapServiceExternalServicesModule(appSettings));

                services.RegisterMtDataReaderClient(
                    appSettings.Nested(x => x.MarginTradingOvernightSwapService.Services.DataReader.Url).CurrentValue,
                    appSettings.Nested(x => x.MarginTradingOvernightSwapService.Services.DataReader.ApiKey).CurrentValue,
                    nameof(OvernightSwapService));
                
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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

#if DEBUG
                app.UseLykkeMiddleware(ServiceName, ex => ex.ToString());
#else
                app.UseLykkeMiddleware(ServiceName, ex => new ErrorResponse {ErrorMessage = "Technical problem", Details
 = ex.Message});
#endif

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

        private async Task StartApplication()
        {
            try
            {
                //TODO thats only to get swap comission values - EOD keeper will provide interest rates
                var accountAssets = (await ApplicationContainer.Resolve<IAccountAssetPairsRepository>().GetAllAsync()).ToList();
                ApplicationContainer.Resolve<IAccountAssetsCacheService>().InitAccountAssetsCache(accountAssets);
                
                // NOTE: Service not yet recieves and processes requests here
                var settingsCalcTime = ApplicationContainer.Resolve<IReloadingManager<MarginTradingOvernightSwapServiceSettings>>()
                    .CurrentValue.OvernightSwapCalculationTime;
                var registry = new Registry();
                registry.Schedule<OvernightSwapJob>().ToRunEvery(0).Days().At(settingsCalcTime.Hours, 
                    settingsCalcTime.Minutes);
                JobManager.Initialize(registry);
                JobManager.JobException += info => Log.WriteError(ServiceName, nameof(JobManager), info.Exception);

                ApplicationContainer.Resolve<IOvernightSwapService>().Start();
                
                Log?.WriteMonitorAsync("", "", "Started").Wait();
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex).Wait();
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can recieve and process requests here, so take care about it if you add logic here.
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
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources

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

        private static ILog CreateLog(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            return aggregateLogger;
        }
    }
}
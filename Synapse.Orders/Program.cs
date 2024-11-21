using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Synapse.Orders.Configuration;
using Synapse.Orders.Configuration.Interfaces;
using Synapse.Orders.Services;
using Synapse.Orders.Services.Interfaces;


var app = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(ConfigureAppConfiguration)
    .ConfigureLogging(ConfigureLogging)
    .ConfigureServices(ConfigureServices)
    .ConfigureWebHostDefaults(wb =>
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var synapseOptions = SynapseOptions.NewInstance();
            configuration.GetSection(Constants.SynapseOrderMonitorKey).Bind(synapseOptions);
        })
    .Build();

await app.RunAsync();


static void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder configuration)
{
    var env = hostContext.HostingEnvironment;

    configuration.SetBasePath(Directory.GetCurrentDirectory());
    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

    configuration.AddEnvironmentVariables();
}


static void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder builder)
{
    var serilogLogger = new LoggerConfiguration()
        .MinimumLevel.Debug() // Set the minimum log level
        .Enrich.FromLogContext()
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information)
                            .Filter.ByExcluding(e => e.Properties.ToString().Contains("Microsoft"))
                            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Fatal)
                            .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Fatal)
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
                            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
        .WriteTo.Logger(lc => lc
                            .Filter.ByIncludingOnly(evt => evt.Level is LogEventLevel.Debug or > LogEventLevel.Information)
                            .Filter.ByExcluding(e => e.Properties.ToString().Contains("Microsoft"))
                            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Fatal)
                            .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Fatal)
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
                            .WriteTo.File("logs/app-.log",
                                          rollingInterval: RollingInterval.Day,
                                          retainedFileCountLimit: 7,
                                          shared: true,
                                          flushToDiskInterval: TimeSpan.FromSeconds(1),
                                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
        .CreateLogger();


    builder.ClearProviders();
    builder.AddSerilog(serilogLogger, dispose: true);
}


static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
{
    var redditOptions = SynapseOptions.NewInstance();
    var configurationSection = hostContext.Configuration.GetSection(Constants.SynapseOrderMonitorKey);

    services.Configure<SynapseOptions>(configurationSection);
    configurationSection.Bind(redditOptions);

    services.AddOptions<SynapseOptions>()
        .Bind(hostContext.Configuration.GetSection(Constants.SynapseOrderMonitorKey))
        .Validate(op => !string.IsNullOrEmpty(op.OrdersBaseUrl), "OrdersBaseUrl must not be empty.")
        .Validate(op => !string.IsNullOrEmpty(op.AlertBaseUrl), "AlertBaseUrl must not be empty.")
        .Validate(op => !string.IsNullOrEmpty(op.UpdateBaseUrl), "UpdateBaseUrl must not be empty.");

    services.AddHostedService<SynapseMonitoringService>();

    services.AddHttpClient("SynpaseHttpClient", client => { client.DefaultRequestHeaders.Add("Accept", "application/json"); });
    services.AddScoped<ISynpaseAlertsService, SynpaseAlertsService>();
    services.AddScoped<ISynpaseEquipmentOrdersService, SynpaseEquipmentOrdersService>();
    services.AddScoped<ISynpaseUpdateorderService, SynpaseUpdateorderService>();
    



}
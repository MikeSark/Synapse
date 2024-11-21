var app = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(ConfigureAppConfiguration)
    .ConfigureLogging(ConfigureLogging)
    .ConfigureServices(ConfigureServices)
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
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Information)
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
                            .WriteTo.File("logs/synapse_orders.log",
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

    services.AddHttpClient("SynpaseHttpClient", client => { client.DefaultRequestHeaders.Add("Accept", "application/json"); });
    services.AddScoped<ISynpaseAlertsService, SynpaseAlertsService>();
    services.AddScoped<ISynpaseEquipmentOrdersService, SynpaseEquipmentOrdersService>();
    services.AddScoped<ISynpaseUpdateorderService, SynpaseUpdateorderService>();


    /*
        to simplify the demo, I have moved the functionlity from source code provided to me
        to a few service classes.
    
        I added a new monitoring class that implements IHostedLifetimeService, which runs indefinetly,
        check for orders and process them as per instructions in the source code provided, pause for 
        a specific time in secodns identified in appsettings.json file. 
    
        The Monitoring repeats the process until the service is stopped. there are few ways that we can deploy this
        in a production environment
    
        1. Simply run this as an autostart executable on target machine
        2. implement a Windows Service that starts and stops the monitoring service        
        3. As a Docker Container, Kubernetes Pod, 
        4. As a Azure Function, AWS Lambda Function, Google Cloud Function
        8. As a Azure WebJob     
    
    */
    services.AddHostedService<SynapseMonitoringService>();
}
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

        The functionality originally provided in the source code has been modularized into a set of
        service classes for improved maintainability and scalability.

        A new monitoring service has been introduced, implementing the IHostedService interface. This service
        operates as a long-running background process, continuously checking for orders and processing them based on
        the logic outlined in the provided source code.

        Between each processing cycle, the service pauses for a configurable duration specified in the
        appsettings.json file. This process repeats indefinitely until the service is explicitly stopped.

        There are several deployment strategies available for running this monitoring service in a production environment:

        Autostart Executable: Deploy the service as an executable configured to run automatically on the target machine at startup.

        Windows Service:    Implement the monitoring service as a Windows Service, allowing for managed lifecycle
                            operations such as start, stop, and restart through the Windows Service Manager.

        Docker Container
        Kubernetes Pod:     Package the service in a Docker container and deploy it either as a standalone container
                            or as part of a Kubernetes pod for scalable and containerized deployment.

        Serverless Function:
            Azure Function
            AWS Lambda Function
            Google Cloud Function

        Azure WebJob:   Deploy the service as a WebJob in Azure, leveraging Azure App Services for continuous or triggered background task execution.

    */

    services.AddHostedService<SynapseMonitoringService>();
}
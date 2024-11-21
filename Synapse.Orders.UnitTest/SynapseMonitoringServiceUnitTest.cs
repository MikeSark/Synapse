namespace Synapse.Orders.UnitTest;

public class SynapseMonitoringServiceTest
{
    private readonly AutoMocker _mocker;
    private readonly SynapseMonitoringService _service;

    public SynapseMonitoringServiceTest()
    {
        _mocker = new AutoMocker();

        var options = SynapseOptions.NewInstance();
        options.RefreshInterval = 10;

        _mocker.Use<IOptions<SynapseOptions>>(Options.Create(options));
        _service = _mocker.CreateInstance<SynapseMonitoringService>();
    }

    [Fact]
    public async Task started_async_should_process_orders()
    {
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var equipmentOrdersServiceMock = _mocker.GetMock<ISynpaseEquipmentOrdersService>();
        var updateOrderServiceMock = _mocker.GetMock<ISynpaseUpdateorderService>();
        var alertsServiceMock = _mocker.GetMock<ISynpaseAlertsService>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;


        var order = new JObject { ["OrderId"] = "123", ["Items"] = new JArray(new JObject { ["Status"] = "Delivered" }) };
        equipmentOrdersServiceMock.Setup(service => service.FetchMedicalEquipmentOrders()).ReturnsAsync(new[] { order });
        updateOrderServiceMock.Setup(service => service.UpdateEquipmentOrder(It.IsAny<JObject>())).ReturnsAsync(true);
        alertsServiceMock.Setup(service => service.SendAlertMessage(It.IsAny<JToken>(), It.IsAny<string>())).ReturnsAsync(true);


        var task = _service.StartedAsync(cancellationToken);
        cancellationTokenSource.CancelAfter(100); // Cancel after a short delay to stop the loop


        await task;


        equipmentOrdersServiceMock.Verify(service => service.FetchMedicalEquipmentOrders(), Times.AtLeastOnce);
        updateOrderServiceMock.Verify(service => service.UpdateEquipmentOrder(It.IsAny<JObject>()), Times.AtLeastOnce);
        alertsServiceMock.Verify(service => service.SendAlertMessage(It.IsAny<JToken>(), It.IsAny<string>()), Times.AtLeastOnce);
        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StartedAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }


    [Fact]
    public async Task start_async_should_log_debug_message()
    {
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        await _service.StartAsync(cancellationToken);

        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StartAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task starting_async_should_log_debug_message()
    {
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        await _service.StartingAsync(cancellationToken);

        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StartingAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }


    [Fact]
    public async Task stop_async_should_log_debug_message()
    {
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        await _service.StopAsync(cancellationToken);

        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StopAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task stopping_async_should_log_debug_message()
    {
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        await _service.StoppingAsync(cancellationToken);

        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StoppingAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task stopped_async_should_log_debug_message()
    {
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        await _service.StoppedAsync(cancellationToken);

        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StoppedAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
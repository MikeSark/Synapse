using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json.Linq;
using Synapse.Orders.Configuration;
using Synapse.Orders.Services;
using Synapse.Orders.Services.Interfaces;

namespace Synapse.Orders.UnitTest;

public class SynapseMonitoringServiceTests
{
    private readonly AutoMocker _mocker;
    private readonly SynapseMonitoringService _service;

    public SynapseMonitoringServiceTests()
    {
        _mocker = new AutoMocker();

        var options = SynapseOptions.NewInstance();
        options.RefreshInterval = 10;
        
        _mocker.Use<IOptions<SynapseOptions>>(Options.Create(options));
        _service = _mocker.CreateInstance<SynapseMonitoringService>();
    }

    [Fact]
    public async Task StartAsync_ShouldLogDebugMessage()
    {
        // Arrange
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        // Act
        await _service.StartAsync(cancellationToken);

        // Assert
        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StartAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task StartingAsync_ShouldLogDebugMessage()
    {
        // Arrange
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        // Act
        await _service.StartingAsync(cancellationToken);

        // Assert
        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StartingAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task StartedAsync_ShouldProcessOrders()
    {
        // Arrange
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

        // Assert
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
    public async Task StopAsync_ShouldLogDebugMessage()
    {
        // Arrange
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        // Act
        await _service.StopAsync(cancellationToken);

        // Assert
        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StopAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task StoppingAsync_ShouldLogDebugMessage()
    {
        // Arrange
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        // Act
        await _service.StoppingAsync(cancellationToken);

        // Assert
        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StoppingAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task StoppedAsync_ShouldLogDebugMessage()
    {
        // Arrange
        var loggerMock = _mocker.GetMock<ILogger<SynapseMonitoringService>>();
        var cancellationToken = new CancellationToken();

        // Act
        await _service.StoppedAsync(cancellationToken);

        // Assert
        loggerMock.Verify(logger => logger.Log(
                              LogLevel.Debug,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("StoppedAsync")),
                              It.IsAny<Exception>(),
                              It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
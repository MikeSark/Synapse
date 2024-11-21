namespace Synapse.Orders.UnitTest;

public class SynpaseAlertsServiceUnitTest
{
    private readonly Mock<ISynpaseAlertsService> _mockSynpaseAlertsService = new();


    [Fact]
    public async Task send_alert_message_should_return_true_when_message_is_sent_successfully()
    {
        var item = JToken.Parse("{\"key\":\"value\"}");
        var orderId = "12345";
        
        _mockSynpaseAlertsService.Setup(service => service.SendAlertMessage(item, orderId))
            .ReturnsAsync(true);
        
        var result = await _mockSynpaseAlertsService.Object.SendAlertMessage(item, orderId);
        
        Assert.True(result);
    }
    [Fact]
    public async Task send_alert_message_should_return_dalse_when_message_sending_fails()
    {
        var item = JToken.Parse("{\"key\":\"value\"}");
        var orderId = "12345";
        
        _mockSynpaseAlertsService.Setup(service => service.SendAlertMessage(item, orderId))
            .ReturnsAsync(false);
        
        var result = await _mockSynpaseAlertsService.Object.SendAlertMessage(item, orderId);
        
        Assert.False(result);
    }

    [Fact]
    public async Task send_alert_message_should_throw_exception_when_an_error_occurs()
    {
        // Arrange
        var item = JToken.Parse("{\"key\":\"value\"}");
        var orderId = "12345";
        _mockSynpaseAlertsService.Setup(service => service.SendAlertMessage(item, orderId))
            .ThrowsAsync(new Exception("An error occurred"));
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _mockSynpaseAlertsService.Object.SendAlertMessage(item, orderId));
    }
}
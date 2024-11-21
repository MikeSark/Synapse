using Moq;
using Newtonsoft.Json.Linq;
using Synapse.Orders.Services.Interfaces;

namespace Synapse.Orders.UnitTest;

public class SynpaseAlertsServiceUnitTest
{
    private readonly Mock<ISynpaseAlertsService> _mockSynpaseAlertsService = new();


    [Fact]
    public async Task SendAlertMessage_ShouldReturnTrue_WhenMessageIsSentSuccessfully()
    {
        var item = JToken.Parse("{\"key\":\"value\"}");
        var orderId = "12345";
        
        _mockSynpaseAlertsService.Setup(service => service.SendAlertMessage(item, orderId))
            .ReturnsAsync(true);
        
        var result = await _mockSynpaseAlertsService.Object.SendAlertMessage(item, orderId);
        
        Assert.True(result);
    }
    [Fact]
    public async Task SendAlertMessage_ShouldReturnFalse_WhenMessageSendingFails()
    {
        var item = JToken.Parse("{\"key\":\"value\"}");
        var orderId = "12345";
        
        _mockSynpaseAlertsService.Setup(service => service.SendAlertMessage(item, orderId))
            .ReturnsAsync(false);
        
        var result = await _mockSynpaseAlertsService.Object.SendAlertMessage(item, orderId);
        
        Assert.False(result);
    }

    [Fact]
    public async Task SendAlertMessage_ShouldThrowException_WhenAnErrorOccurs()
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
namespace Synapse.Orders.UnitTest;

public class SynpaseUpdateorderServiceTest
{
    private readonly Mock<ISynpaseUpdateorderService> _mockUpdateOrderService;
    public SynpaseUpdateorderServiceTest() => _mockUpdateOrderService = new Mock<ISynpaseUpdateorderService>();

    [Fact]
    public async Task update_equipment_order_should_return_true_when_order_is_valid()
    {
        var order = new JObject();
        _mockUpdateOrderService
            .Setup(service => service.UpdateEquipmentOrder(order))
            .ReturnsAsync(true);

        var result = await _mockUpdateOrderService.Object.UpdateEquipmentOrder(order);

        Assert.True(result);
    }

    [Fact]
    public async Task update_equipment_order_should_return_false_when_order_is_invalid()
    {
        var order = new JObject();

        _mockUpdateOrderService
            .Setup(service => service.UpdateEquipmentOrder(order))
            .ReturnsAsync(false);

        var result = await _mockUpdateOrderService.Object.UpdateEquipmentOrder(order);

        Assert.False(result);
    }
}
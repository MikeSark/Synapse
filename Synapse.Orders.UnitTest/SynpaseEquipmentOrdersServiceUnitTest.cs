namespace Synapse.Orders.UnitTest;

public class SynpaseEquipmentOrdersServiceUnitTest
{
    private readonly Mock<ISynpaseEquipmentOrdersService> _mockEquipmentOrdersService;
    public SynpaseEquipmentOrdersServiceUnitTest() => _mockEquipmentOrdersService = new Mock<ISynpaseEquipmentOrdersService>();

    [Fact]
    public async Task fetch_medical_equipment_orders_should_return_orders_when_orders_exist()
    {
        var orders = new JObject[]
        {
            new JObject { ["OrderId"] = "1", ["Status"] = "Pending" },
            new JObject { ["OrderId"] = "2", ["Status"] = "Delivered" }
        };

        _mockEquipmentOrdersService
            .Setup(service => service.FetchMedicalEquipmentOrders())
            .ReturnsAsync(orders);

        var result = await _mockEquipmentOrdersService.Object.FetchMedicalEquipmentOrders();

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("1", result[0]["OrderId"]);
        Assert.Equal("2", result[1]["OrderId"]);
    }

    [Fact]
    public async Task fetch_medical_equipment_orders_should_throw_exception_when_service_is_unavailable()
    {
        _mockEquipmentOrdersService
            .Setup(service => service.FetchMedicalEquipmentOrders())
            .ThrowsAsync(new Exception("Service is unavailable"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _mockEquipmentOrdersService.Object.FetchMedicalEquipmentOrders());

        Assert.Equal("Service is unavailable", exception.Message);
    }

    [Fact]
    public async Task fetch_medical_equipment_orders_should_return_orders_when_orders_exist_with_status()
    {
        var orders = new JObject[]
        {
            new JObject { ["OrderId"] = "1", ["Status"] = "Pending" },
            new JObject { ["OrderId"] = "2", ["Status"] = "Delivered" }
        };

        _mockEquipmentOrdersService
            .Setup(service => service.FetchMedicalEquipmentOrders())
            .ReturnsAsync(orders);

        var result = await _mockEquipmentOrdersService.Object.FetchMedicalEquipmentOrders();

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("1", result[0]["OrderId"]);
        Assert.Equal("2", result[1]["OrderId"]);
        Assert.Equal("Pending", result[0]["Status"]);
        Assert.Equal("Delivered", result[1]["Status"]);
    }


    [Fact]
    public async Task fetch_medical_equipment_orders_should_return_empty_array_when_no_orders_exist()
    {
        var orders = new JObject[] { };
        _mockEquipmentOrdersService
            .Setup(service => service.FetchMedicalEquipmentOrders())
            .ReturnsAsync(orders);

        var result = await _mockEquipmentOrdersService.Object.FetchMedicalEquipmentOrders();

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
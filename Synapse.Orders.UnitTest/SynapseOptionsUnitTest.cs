using Synapse.Orders.Configuration;
using Synapse.Orders.Configuration.Interfaces;

namespace Synapse.Orders.UnitTest;

public class SynapseOptionsUnitTest
{
    [Fact]
    public void synapse_options_should_implement_iSynapse_options()
    {
        // Arrange
        var options = SynapseOptions.NewInstance();

        // Assert
        Assert.IsAssignableFrom<ISynapseOptions>(options);
    }


    [Fact]
    public void synapse_options_should_have_default_values()
    {
        // Arrange
        var options = SynapseOptions.NewInstance();

        // Assert
        Assert.Equal(string.Empty, options.AppName);
        Assert.Equal(10, options.RefreshInterval);
        Assert.Equal(string.Empty, options.OrdersBaseUrl);
        Assert.Equal(string.Empty, options.AlertBaseUrl);
        Assert.Equal(string.Empty, options.UpdateBaseUrl);
    }
        
    [Fact]
    public void synapse_options_should_have_configuration_with_some_values()
    {
        // Arrange
        var options = SynapseOptions.NewInstance();

        // Act
        options.AppName = "Synapse Orders";
        options.RefreshInterval = 15;
        options.OrdersBaseUrl = "https://orders.synapse.com";
        options.AlertBaseUrl = "https://alerts.synapse.com";
        options.UpdateBaseUrl = "https://updates.synapse.com";

        // Assert
        Assert.Equal("Synapse Orders", options.AppName);
        Assert.Equal(15, options.RefreshInterval);
        Assert.Equal("https://orders.synapse.com", options.OrdersBaseUrl);
        Assert.Equal("https://alerts.synapse.com", options.AlertBaseUrl);
        Assert.Equal("https://updates.synapse.com", options.UpdateBaseUrl);
    }
}
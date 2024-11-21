namespace Synapse.Orders.Configuration;

public class SynapseOptions : ISynapseOptions
{
    public static SynapseOptions NewInstance() =>
        new SynapseOptions()
        {
            AlertBaseUrl = string.Empty,
            OrdersBaseUrl = string.Empty,
            UpdateBaseUrl = string.Empty
        };

    public string? AppName { get; set; } = string.Empty;
    public int RefreshInterval { get; set; } = 10;
    public required string OrdersBaseUrl { get; set; } = string.Empty;
    public required string AlertBaseUrl { get; set; } = string.Empty;
    public required string UpdateBaseUrl { get; set; } = string.Empty;
    
}
namespace Synapse.Orders.Configuration.Interfaces;

public interface ISynapseOptions
{
    string? AppName { get; set; }
    int RefreshInterval { get; set; }
    public string? OrdersBaseUrl { get; set; }
    public string? AlertBaseUrl { get; set; }
    public string? UpdateBaseUrl { get; set; }
}
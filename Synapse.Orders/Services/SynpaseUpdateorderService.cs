using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Synapse.Orders.Configuration;
using Synapse.Orders.Services.Interfaces;

namespace Synapse.Orders.Services;

public class SynpaseUpdateorderService : ISynpaseUpdateorderService
{
    private readonly ILogger<SynpaseAlertsService> _logger;
    private readonly SynapseOptions _synapseOptions;
    private readonly HttpClient _httpClient;

    public SynpaseUpdateorderService(ILogger<SynpaseAlertsService> logger,
                                     IOptions<SynapseOptions> redditOptions, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _synapseOptions = redditOptions.Value;
        _httpClient = httpClientFactory.CreateClient();
    }


    public async Task<bool> UpdateEquipmentOrder(JObject order)
    {
        _logger.LogInformation("Calling endpoint for updating the order.");
        try
        {
            var urlPath = $"{_synapseOptions.UpdateBaseUrl}{Constants.UpdateRouteKey}";
            var content = new StringContent(order.ToString(), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(urlPath, content);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Updated order sent for processing: OrderId {order["OrderId"]}");
                return true;
            }

            _logger.LogError($"Failed to send updated order for processing: OrderId {order["OrderId"]}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message, exception);
        }

        return false;
    }
}
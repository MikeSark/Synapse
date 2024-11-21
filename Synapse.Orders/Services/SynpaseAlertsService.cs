using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Synapse.Orders.Configuration;
using Synapse.Orders.Services.Interfaces;

namespace Synapse.Orders.Services;

public class SynpaseAlertsService : ISynpaseAlertsService
{
    private readonly ILogger<SynpaseAlertsService> _logger;
    private readonly SynapseOptions _synapseOptions;
    private readonly HttpClient _httpClient;

    public SynpaseAlertsService(ILogger<SynpaseAlertsService> logger,
                                IOptions<SynapseOptions> redditOptions, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _synapseOptions = redditOptions.Value;
        _httpClient = httpClientFactory.CreateClient();
    }


    public async Task<bool> SendAlertMessage(JToken item, string orderId)
    {
        _logger.LogInformation("Calling endpoint for Alerts.");
        try
        {
            var urlPath = $"{_synapseOptions.AlertBaseUrl}{Constants.AlertsRouteKey}";
            var alertData = new
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                          $"Delivery Notifications: {item["deliveryNotification"]}"
            };
            var content = new StringContent(JObject.FromObject(alertData).ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(urlPath, content);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Alert sent for delivered item: {item["Description"]}");
                return true;
            }

            _logger.LogError($"Failed to send alert for delivered item: {item["Description"]}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message, exception);
        }

        return false;
    }
}
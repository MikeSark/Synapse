using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Synapse.Orders.Configuration;
using Synapse.Orders.Services.Interfaces;

namespace Synapse.Orders.Services;

public class SynpaseEquipmentOrdersService : ISynpaseEquipmentOrdersService
{
    private readonly ILogger<SynpaseEquipmentOrdersService> _logger;
    private readonly SynapseOptions _synapseOptions;
    private readonly HttpClient _httpClient;

    public SynpaseEquipmentOrdersService(ILogger<SynpaseEquipmentOrdersService> logger,
                                         IOptions<SynapseOptions> redditOptions, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _synapseOptions = redditOptions.Value;
        _httpClient = httpClientFactory.CreateClient();
    }


    public async Task<JObject[]> FetchMedicalEquipmentOrders()
    {
        _logger.LogInformation("Calling endpoint for MedicalEquipment Orders");

        try
        {
            var response = await _httpClient.GetAsync($"{_synapseOptions.OrdersBaseUrl}{Constants.OrdersRouteKey}");
            if (response.IsSuccessStatusCode)
            {
                var ordersData = await response.Content.ReadAsStringAsync();
                var jarrayData = JArray.Parse(ordersData).ToObject<JObject[]>();

                _logger.LogInformation($"Service call returned {jarrayData?.Length ?? 0} Orders.");
                return jarrayData ?? [];
            }

            var error = await GetResponseErrorMessage(response);
            _logger.LogError($"Failed to fetch orders from API. {error}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message, exception);
        }

        return [];

        async Task<string> GetResponseErrorMessage(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return response.StatusCode switch
                   {
                       System.Net.HttpStatusCode.NotFound => $"Resource not found. Details: {errorContent}",
                       System.Net.HttpStatusCode.BadRequest => $"Bad Request. Details: {errorContent}",
                       System.Net.HttpStatusCode.InternalServerError => $"Server error occurred. Details {errorContent}",
                       _ => $"Error ({response.StatusCode}): {errorContent}"
                   };
        }
    }
}
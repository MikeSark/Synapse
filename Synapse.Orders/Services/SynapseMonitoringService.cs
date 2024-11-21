using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Synapse.Orders.Configuration;
using Synapse.Orders.Services.Interfaces;

namespace Synapse.Orders.Services;

public class SynapseMonitoringService : IHostedLifecycleService
{
    private readonly ILogger<SynapseMonitoringService> _logger;
    private readonly SynapseOptions _synapseOptions;


    private readonly ISynpaseEquipmentOrdersService _synpaseEquipmentOrdersService;
    private readonly ISynpaseUpdateorderService _synpaseUpdateorderService;
    private readonly ISynpaseAlertsService _synpaseAlertsService;

    public SynapseMonitoringService(ILogger<SynapseMonitoringService> logger, IOptions<SynapseOptions> synapseOptions,
                                    ISynpaseEquipmentOrdersService synpaseEquipmentOrdersService,
                                    ISynpaseUpdateorderService synpaseUpdateorderService,
                                    ISynpaseAlertsService synpaseAlertsService)
    {
        _logger = logger;
        _synapseOptions = synapseOptions.Value;

        _synpaseEquipmentOrdersService = synpaseEquipmentOrdersService;
        _synpaseUpdateorderService = synpaseUpdateorderService;
        _synpaseAlertsService = synpaseAlertsService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("StartAsync");
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("StartingAsync");
        return Task.CompletedTask;
    }


    /// <summary>
    /// A long running task that fetches medical equipment orders, processes them and sends alerts.
    /// in this example we will not make use of Cancellation token to stop the process.
    /// However we could use it to stop the process by implementation a running task.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("StartedAsync");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var medicalEquipmentOrders = await _synpaseEquipmentOrdersService.FetchMedicalEquipmentOrders();
                foreach (var order in medicalEquipmentOrders)
                {
                    var updatedOrder = await ProcessOrder(order);
                    await _synpaseUpdateorderService.UpdateEquipmentOrder(updatedOrder);
                }

                // wait xx seconds and retry again..
                await Task.Delay((_synapseOptions.RefreshInterval * 1000), cancellationToken);

                _logger.LogDebug($"In the Loop: {DateTime.Now:F}");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("Task Cancellation was requested for StartedAsync");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("StopAsync");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("StoppingAsync");
        return Task.CompletedTask;
    }


    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("StoppedAsync");
        return Task.CompletedTask;
    }

    #region private members

    private async Task<JObject> ProcessOrder(JObject order)
    {
        var itemsToken = order["Items"];
        if (itemsToken is { Type: JTokenType.Array })
        {
            var items = itemsToken.ToObject<JArray>();
            foreach (var item in items!)
            {
                if (!IsItemDelivered(item)) continue;

                var orderIdToken = order["OrderId"];
                if (orderIdToken != null)
                {
                    await _synpaseAlertsService.SendAlertMessage(item, orderIdToken.ToString());
                    IncrementDeliveryNotification(item);
                }
                else
                {
                    // Handle the case where "OrderId" is null
                    _logger.LogWarning("Order does not contain a valid 'OrderId'.");
                }
            }
        }
        else
        {
            // Handle the case where "Items" is null or not an array
            _logger.LogWarning("Order does not contain a valid 'Items' array.");
        }

        return order;
    }

    static bool IsItemDelivered(JToken item) =>
        item["Status"]?.ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase) ?? false;


    static void IncrementDeliveryNotification(JToken item) =>
        item["deliveryNotification"] = (item["deliveryNotification"]?.Value<int>() ?? 0) + 1;

    #endregion
}
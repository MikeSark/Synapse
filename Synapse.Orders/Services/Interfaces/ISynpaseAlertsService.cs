using Newtonsoft.Json.Linq;

namespace Synapse.Orders.Services.Interfaces;

public interface ISynpaseAlertsService
{
    Task<bool> SendAlertMessage(JToken item, string orderId);
}
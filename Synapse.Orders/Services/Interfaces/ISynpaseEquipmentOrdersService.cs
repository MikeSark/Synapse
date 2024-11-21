using Newtonsoft.Json.Linq;

namespace Synapse.Orders.Services.Interfaces;

public interface ISynpaseEquipmentOrdersService
{
    Task<JObject[]> FetchMedicalEquipmentOrders();
}
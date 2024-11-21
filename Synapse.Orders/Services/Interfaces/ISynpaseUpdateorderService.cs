namespace Synapse.Orders.Services.Interfaces;

public interface ISynpaseUpdateorderService
{
    Task<bool> UpdateEquipmentOrder(JObject order);
}
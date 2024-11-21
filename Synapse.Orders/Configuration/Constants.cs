using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Orders.Configuration;

public sealed class Constants
{
    public const string New = "New";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Canceled = "Canceled";

    public const string SynapseOrderMonitorKey = "SynapseOrderMonitor";
    public const string OrdersBaseUrlKey = "ordersBaseUrl";
    public const string AlertBaseUrlKey = "alertBaseUrl";
    public const string UpdateBaseUrlKey = "updateBaseUrl";


    public const string UpdateRouteKey = "/update";
    public const string AlertsRouteKey = "/alerts";
    public const string OrdersRouteKey = "/orders";
}
using System.Web.Http;

namespace PSVeeamRESTAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "Veeam Replication Resource",
                routeTemplate: "api/veeam/{vbrHost}/resources/{tenantUid}/replications/{resourceUid}",
                defaults: new { controller = "veeamreplicationresource",
                    tenantUid=RouteParameter.Optional, resourceUid=RouteParameter.Optional}
            );
        }
    }
}

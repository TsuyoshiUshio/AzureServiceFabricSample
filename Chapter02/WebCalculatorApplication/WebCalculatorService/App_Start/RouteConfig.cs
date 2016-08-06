
namespace WebCalculatorService
{
    using System.Web.Http;
    public static class RouteConfig
    {
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "CalculatorApi",
                routeTemplate: "api/{action}",
                defaults: new { controller = "Default" }
                );
        }
    }
}

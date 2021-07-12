using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.BillingCountry
{
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.DiscountRules.BillingCountry.Configure",
                 "Plugins/DiscountRulesBillingCountry/Configure",
                 new { controller = "DiscountRulesBillingCountry", action = "Configure" });
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }

        #endregion
    }
}

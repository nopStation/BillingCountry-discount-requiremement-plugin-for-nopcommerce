using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.BillingCountry.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableCountries = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.DiscountRules.BillingCountry.Fields.Country")]
        public int CountryId { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}
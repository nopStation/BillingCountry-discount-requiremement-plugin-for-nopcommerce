using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.BillingCountry.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Nop.Plugin.DiscountRules.BillingCountry.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DiscountRulesBillingCountryController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IDiscountService _discountService;
        private readonly ICountryService _countryService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public DiscountRulesBillingCountryController(ILocalizationService localizationService, 
            IDiscountService discountService, 
            ICountryService countryService,
            ISettingService settingService, 
            IPermissionService permissionService)
        {
            this._localizationService = localizationService;
            this._discountService = discountService;
            this._countryService = countryService;
            this._settingService = settingService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
            {
                discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value);
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var billingCountryId = await _settingService.GetSettingByKeyAsync<int>($"DiscountRequirement.BillingCountry-{discountRequirementId ?? 0}");

            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                CountryId = billingCountryId
            };

            //countries
            model.AvailableCountries.Add(new SelectListItem() { Text = await _localizationService.GetResourceAsync("Plugins.DiscountRules.BillingCountry.Fields.SelectCountry"), Value = "0" });
            foreach (var c in await _countryService.GetAllCountriesAsync(showHidden: true))
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = discountRequirement != null && c.Id == billingCountryId });

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = $"DiscountRulesBillingCountry{discountRequirementId?.ToString() ?? "0"}";

            return View("~/Plugins/DiscountRules.BillingCountry/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId, int countryId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value);

            if (discountRequirement != null)
            {
                //update existing rule
                await _settingService.SetSettingAsync($"DiscountRequirement.BillingCountry-{discountRequirement.Id}", countryId);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.BillingCountryIs"
                };
                await _discountService.InsertDiscountRequirementAsync(discountRequirement);
                
                await _settingService.SetSettingAsync($"DiscountRequirement.BillingCountry-{discountRequirement.Id}", countryId);
            }

            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        #endregion
    }
}
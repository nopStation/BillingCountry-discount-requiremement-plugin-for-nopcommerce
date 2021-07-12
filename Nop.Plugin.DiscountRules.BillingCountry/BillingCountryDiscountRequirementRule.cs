using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using System.Threading.Tasks;
using Nop.Services.Common;

namespace Nop.Plugin.DiscountRules.BillingCountry
{
    public partial class BillingCountryDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public BillingCountryDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            ISettingService settingService,
            ILocalizationService localizationService,
            IUrlHelperFactory urlHelperFactory,
            IAddressService addressService)
        {
            _actionContextAccessor = actionContextAccessor;
            _settingService = settingService;
            _localizationService = localizationService;
            _urlHelperFactory = urlHelperFactory;
            _addressService = addressService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            var billingAddress = await _addressService.GetAddressByIdAsync(request.Customer.BillingAddressId ?? 0);
            if (billingAddress == null)
                return result;

            var billingCountryId = await _settingService.GetSettingByKeyAsync<int>($"DiscountRequirement.BillingCountry-{request.DiscountRequirementId}");

            if (billingCountryId == 0)
                return result;

            result.IsValid = billingAddress.CountryId == billingCountryId;

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>        
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.Action("Configure", "DiscountRulesBillingCountry",
                new { discountId = discountId, discountRequirementId = discountRequirementId }));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            return url.Value.TrimStart('/');
        }

        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.DiscountRules.BillingCountry.Fields.SelectCountry", "Select country");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.DiscountRules.BillingCountry.Fields.Country", "Billing country");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.DiscountRules.BillingCountry.Fields.Country.Hint", "Select required billing country.");
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //locales
            await _localizationService.DeleteLocaleResourceAsync("Plugins.DiscountRules.BillingCountry.Fields.SelectCountry");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.DiscountRules.BillingCountry.Fields.Country");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.DiscountRules.BillingCountry.Fields.Country.Hint");
            await base.UninstallAsync();
        }

        #endregion
    }
}
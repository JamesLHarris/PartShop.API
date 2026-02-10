using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests.ShippingPolicies
{
    public class ShippingPolicyAddRequest
    {
        [Required, StringLength(128, MinimumLength = 2)]
        public string Name { get; set; }

        // Shopify profile ID (can be null until you map it)
        public int? ShopifyProfileId { get; set; }

        // Optional, defaults true at DB layer if you want
        public bool IsActive { get; set; } = true;
    }
}


using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Models
{
    public class ShopifyWebhookRegisterRequest
    {
        [Required]
        public string CallbackUrl { get; set; } = string.Empty;
    }
}

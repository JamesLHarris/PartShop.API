using Microsoft.AspNetCore.Http;
using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public interface IShopifyWebhookService
    {
        Task<ShopifyWebhookProcessingResult> ProcessOrdersPaidWebhookAsync(byte[] rawBody, IHeaderDictionary headers);
    }
}

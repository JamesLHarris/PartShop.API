using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public interface IShopifyOrderService
    {
        Task<List<ShopifyOrderSummary>> GetRecentOrdersAsync(int first, string? view);
        Task<ShopifyOrderSyncResult> SyncRecentPaidOrdersAsync(int first, int userId);
    }
}

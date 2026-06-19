using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public class ShopifyPartSyncService : IShopifyPartSyncService
    {
        private readonly IPartService _partService;
        private readonly IShopifyAdminService _shopifyAdminService;

        public ShopifyPartSyncService(
            IPartService partService,
            IShopifyAdminService shopifyAdminService)
        {
            _partService = partService;
            _shopifyAdminService = shopifyAdminService;
        }

        public async Task<ShopifyPartSyncResult> CreateAndSyncProductForPartAsync(int partId)
        {
            Part part = _partService.GetPartById(partId);

            if (part == null)
            {
                throw new ApplicationException($"Part {partId} was not found.");
            }

            if (part.ShopifyProductId.HasValue || part.ShopifyVariantId.HasValue)
            {
                throw new ApplicationException($"Part {partId} already has Shopify IDs.");
            }

            ShopifyCreateProductResult createResult =
                await _shopifyAdminService.CreateProductForPartAsync(part);

            _partService.UpdateShopifyIds(
                partId,
                createResult.ProductId,
                createResult.VariantId,
                createResult.InventoryItemId);

            Part updatedPart = _partService.GetPartById(partId);

            ShopifyProductInventorySyncResult syncResult =
                await _shopifyAdminService.SyncProductDetailsForPartAsync(updatedPart);

            return new ShopifyPartSyncResult
            {
                PartId = partId,
                CreateResult = createResult,
                SyncResult = syncResult
            };
        }
    }
}
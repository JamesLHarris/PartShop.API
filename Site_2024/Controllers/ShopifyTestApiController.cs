using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/shopify/test")]
    [ApiController]
    [Authorize(Policy = "AdminAction")]
    public class ShopifyTestApiController : ControllerBase
    {
        private readonly IShopifyTokenService _shopifyTokenService;
        private readonly IPartService _partService;
        private IShopifyAdminService _shopifyAdminService;
        private readonly ILogger<ShopifyTestApiController> _logger;


        public ShopifyTestApiController(
            IShopifyTokenService shopifyTokenService,
            IPartService partService,
            IShopifyAdminService shopifyAdminService,
            ILogger<ShopifyTestApiController> logger)
        {
            _shopifyTokenService = shopifyTokenService;
            _partService = partService;
            _shopifyAdminService = shopifyAdminService;
            _logger = logger;
        }

        [HttpGet("token")]
        public async Task<ActionResult<ItemResponse<object>>> TestToken()
        {
            try
            {
                string token = await _shopifyTokenService.GetAccessTokenAsync();

                return Ok(new ItemResponse<object>
                {
                    Item = new
                    {
                        Success = true,
                        TokenPreview = token.Length > 8
                            ? $"{token.Substring(0, 4)}...{token.Substring(token.Length - 4)}"
                            : "Token received"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shopify token test failed.");

                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }

        [HttpPost("create-product/{partId:int}")]
        public async Task<ActionResult<ItemResponse<object>>> CreateProductForPart(int partId)
        {
            try
            {
                Part part = _partService.GetPartById(partId);

                if (part == null)
                {
                    return NotFound(new ErrorResponse("Part not found."));
                }

                if (part.ShopifyProductId.HasValue || part.ShopifyVariantId.HasValue)
                {
                    return BadRequest(new ErrorResponse("This part already has Shopify IDs."));
                }

                ShopifyCreateProductResult shopifyResult =
                    await _shopifyAdminService.CreateProductForPartAsync(part);

                _partService.UpdateShopifyIds(
                    partId,
                    shopifyResult.ProductId,
                    shopifyResult.VariantId,
                    shopifyResult.InventoryItemId);

                return Ok(new ItemResponse<object>
                {
                    Item = new
                    {
                        PartId = partId,
                        shopifyResult.ProductGid,
                        shopifyResult.VariantGid,
                        shopifyResult.InventoryItemGid,
                        shopifyResult.ProductId,
                        shopifyResult.VariantId,
                        shopifyResult.InventoryItemId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shopify create product test failed.");
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }

        [HttpGet("locations")]
        public async Task<ActionResult<ItemResponse<List<ShopifyLocationResult>>>> GetLocations()
        {
            try
            {
                List<ShopifyLocationResult> locations =
                    await _shopifyAdminService.GetLocationsAsync();

                return Ok(new ItemResponse<List<ShopifyLocationResult>>
                {
                    Item = locations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shopify get locations test failed.");
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }

        [HttpPost("sync-product-details/{partId:int}")]
        public async Task<ActionResult<ItemResponse<ShopifyProductInventorySyncResult>>> SyncProductDetails(int partId)
        {
            try
            {
                Part part = _partService.GetPartById(partId);

                if (part == null)
                {
                    return NotFound(new ErrorResponse("Part not found."));
                }

                if (!part.ShopifyProductId.HasValue ||
                    !part.ShopifyVariantId.HasValue ||
                    !part.ShopifyInventoryItemId.HasValue)
                {
                    return BadRequest(new ErrorResponse("This part does not have Shopify IDs yet. Create the Shopify product first."));
                }

                ShopifyProductInventorySyncResult result =
                    await _shopifyAdminService.SyncProductDetailsForPartAsync(part);

                return Ok(new ItemResponse<ShopifyProductInventorySyncResult>
                {
                    Item = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shopify product details sync failed.");
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }

        [HttpPost("create-and-sync-product/{partId:int}")]
        public async Task<ActionResult<ItemResponse<object>>> CreateAndSyncProductForPart(int partId)
        {
            try
            {
                Part part = _partService.GetPartById(partId);

                if (part == null)
                {
                    return NotFound(new ErrorResponse("Part not found."));
                }

                if (part.ShopifyProductId.HasValue || part.ShopifyVariantId.HasValue)
                {
                    return BadRequest(new ErrorResponse("This part already has Shopify IDs."));
                }

                ShopifyCreateProductResult createResult =
                    await _shopifyAdminService.CreateProductForPartAsync(part);

                _partService.UpdateShopifyIds(
                    partId,
                    createResult.ProductId,
                    createResult.VariantId,
                    createResult.InventoryItemId);

                // Reload after saving Shopify IDs so the sync method has the IDs.
                Part updatedPart = _partService.GetPartById(partId);

                ShopifyProductInventorySyncResult syncResult =
                    await _shopifyAdminService.SyncProductDetailsForPartAsync(updatedPart);

                return Ok(new ItemResponse<object>
                {
                    Item = new
                    {
                        PartId = partId,
                        CreateResult = createResult,
                        SyncResult = syncResult
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shopify create and sync product test failed.");
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }
    }
}
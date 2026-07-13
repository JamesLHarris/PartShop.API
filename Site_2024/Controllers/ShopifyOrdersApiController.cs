using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/shopify/orders")]
    [ApiController]
    [Authorize(Policy = "AdminAction")]
    public class ShopifyOrdersApiController : ControllerBase
    {
        private readonly IShopifyOrderService _shopifyOrderService;
        private readonly IAuthenticationService<IUserAuthData> _authService;
        private readonly ILogger<ShopifyOrdersApiController> _logger;

        public ShopifyOrdersApiController(
            IShopifyOrderService shopifyOrderService,
            IAuthenticationService<IUserAuthData> authService,
            ILogger<ShopifyOrdersApiController> logger)
        {
            _shopifyOrderService = shopifyOrderService;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("recent")]
        public async Task<ActionResult<ItemResponse<List<ShopifyOrderSummary>>>> GetRecent(
            [FromQuery] int first = 25,
            [FromQuery] string? view = "awaitingShipment")
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<ShopifyOrderSummary> orders = await _shopifyOrderService.GetRecentOrdersAsync(first, view);
                response = new ItemResponse<List<ShopifyOrderSummary>> { Item = orders };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                _logger.LogError(ex, "Failed to load recent Shopify orders.");
            }

            return StatusCode(code, response);
        }

        [HttpPost("sync-recent-paid")]
        public async Task<ActionResult<ItemResponse<ShopifyOrderSyncResult>>> SyncRecentPaid(
            [FromQuery] int first = 25)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                IUserAuthData user = _authService.GetCurrentUser();

                if (user == null)
                {
                    code = 401;
                    response = new ErrorResponse("You must be logged in.");
                    return StatusCode(code, response);
                }

                ShopifyOrderSyncResult result = await _shopifyOrderService.SyncRecentPaidOrdersAsync(first, user.Id);
                response = new ItemResponse<ShopifyOrderSyncResult> { Item = result };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                _logger.LogError(ex, "Failed to sync recent paid Shopify orders.");
            }

            return StatusCode(code, response);
        }
    }
}

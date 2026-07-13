using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/shopify/checkout")]
    [ApiController]
    [AllowAnonymous]
    public class ShopifyCheckoutApiController : ControllerBase
    {
        private readonly IShopifyCheckoutService _checkoutService;
        private readonly ILogger<ShopifyCheckoutApiController> _logger;

        public ShopifyCheckoutApiController(
            IShopifyCheckoutService checkoutService,
            ILogger<ShopifyCheckoutApiController> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        [HttpPost("cart")]
        public ActionResult<ItemResponse<ShopifyCheckoutResult>> CreateCartCheckoutUrl([FromBody] ShopifyCheckoutRequest request)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                ShopifyCheckoutResult result = _checkoutService.CreateCheckoutUrl(request);
                response = new ItemResponse<ShopifyCheckoutResult> { Item = result };
            }
            catch (InvalidOperationException ex)
            {
                code = 400;
                response = new ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse("Unable to start Shopify checkout.");
                _logger.LogError(ex, "Unable to create Shopify checkout URL.");
            }

            return StatusCode(code, response);
        }
    }
}

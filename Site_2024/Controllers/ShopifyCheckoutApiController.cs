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
        public ActionResult<ItemResponse<ShopifyCheckoutResult>>
            CreateCartCheckoutUrl(
                [FromBody] ShopifyCheckoutRequest request)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                ShopifyCheckoutResult result =
                    _checkoutService.CreateCheckoutUrl(request);

                response = new ItemResponse<ShopifyCheckoutResult>
                {
                    Item = result
                };
            }
            catch (InvalidOperationException ex)
            {
                code = 400;
                response = new ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(
                    "Unable to start Shopify checkout.");

                _logger.LogError(
                    ex,
                    "Unable to create Shopify checkout URL.");
            }

            return StatusCode(code, response);
        }

        [HttpGet("status/{checkoutToken}")]
        public ActionResult<ItemResponse<ShopifyCheckoutStatusResult>>
            GetCheckoutStatus(string checkoutToken)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                ShopifyCheckoutStatusResult? result =
                    _checkoutService.GetCheckoutStatus(checkoutToken);

                if (result == null)
                {
                    code = 404;
                    response = new ErrorResponse(
                        "Checkout session was not found.");
                }
                else
                {
                    response =
                        new ItemResponse<ShopifyCheckoutStatusResult>
                        {
                            Item = result
                        };
                }
            }
            catch (InvalidOperationException ex)
            {
                code = 400;
                response = new ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(
                    "Unable to check Shopify checkout status.");

                _logger.LogError(
                    ex,
                    "Unable to load checkout status for token {CheckoutToken}.",
                    checkoutToken);
            }

            return StatusCode(code, response);
        }
    }
}

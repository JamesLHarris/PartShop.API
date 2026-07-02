using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/shopify/webhooks")]
    [ApiController]
    public class ShopifyWebhooksController : ControllerBase
    {
        private readonly IShopifyWebhookService _webhookService;
        private readonly IShopifyWebhookSubscriptionService _subscriptionService;
        private readonly ILogger<ShopifyWebhooksController> _logger;

        public ShopifyWebhooksController(
            IShopifyWebhookService webhookService,
            IShopifyWebhookSubscriptionService subscriptionService,
            ILogger<ShopifyWebhooksController> logger)
        {
            _webhookService = webhookService;
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        // Public endpoint called by Shopify. This endpoint is protected by Shopify HMAC verification,
        // not by Site_2024 cookie authentication.
        [HttpPost("orders-paid")]
        [AllowAnonymous]
        public async Task<IActionResult> OrdersPaid()
        {
            try
            {
                using MemoryStream ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                byte[] rawBody = ms.ToArray();

                ShopifyWebhookProcessingResult result = await _webhookService.ProcessOrdersPaidWebhookAsync(rawBody, Request.Headers);

                if (!result.IsHmacValid)
                {
                    _logger.LogWarning(
                        "Rejected Shopify orders/paid webhook because HMAC verification failed. Topic: {Topic}. Shop: {ShopDomain}.",
                        result.Topic,
                        result.ShopDomain);

                    return Unauthorized();
                }

                // Shopify only needs a 2xx acknowledgement. Do not return customer/order payload data.
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Shopify orders/paid webhook.");

                // Returning 500 lets Shopify retry the webhook delivery.
                return StatusCode(500);
            }
        }

        // Admin helper endpoint to register the ORDERS_PAID webhook through Shopify GraphQL.
        [HttpPost("register-orders-paid")]
        [Authorize(Policy = "AdminAction")]
        public async Task<ActionResult<ItemResponse<ShopifyWebhookSubscriptionResult>>> RegisterOrdersPaid(
            [FromBody] ShopifyWebhookRegisterRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                ShopifyWebhookSubscriptionResult result =
                    await _subscriptionService.RegisterOrdersPaidWebhookAsync(model.CallbackUrl);

                response = new ItemResponse<ShopifyWebhookSubscriptionResult> { Item = result };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                _logger.LogError(ex, "Failed to register Shopify orders/paid webhook.");
            }

            return StatusCode(code, response);
        }

        // Admin helper endpoint to confirm which order webhooks are registered.
        [HttpGet("subscriptions")]
        [Authorize(Policy = "AdminAction")]
        public async Task<ActionResult<ItemResponse<List<ShopifyWebhookSubscriptionInfo>>>> GetSubscriptions()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<ShopifyWebhookSubscriptionInfo> subscriptions =
                    await _subscriptionService.GetOrderWebhookSubscriptionsAsync();

                response = new ItemResponse<List<ShopifyWebhookSubscriptionInfo>> { Item = subscriptions };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                _logger.LogError(ex, "Failed to load Shopify webhook subscriptions.");
            }

            return StatusCode(code, response);
        }
    }
}

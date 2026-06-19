using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/admin/discounts")]
    [ApiController]
    [Authorize(Policy = "AdminAction")]
    public class AdminDiscountCodesApiController : BaseApiController
    {
        private readonly IAdminDiscountCodeService _service;
        private readonly IShopifyAdminService _shopifyAdminService;
        private readonly IAuthenticationService<IUserAuthData> _authService;

        public AdminDiscountCodesApiController(
            IAdminDiscountCodeService service,
            IShopifyAdminService shopifyAdminService,
            IAuthenticationService<IUserAuthData> authService,
            ILogger<AdminDiscountCodesApiController> logger) : base(logger)
        {
            _service = service;
            _shopifyAdminService = shopifyAdminService;
            _authService = authService;
        }

        [HttpPost]
        public async Task<ActionResult<ItemResponse<int>>> Create(AdminDiscountCodeAddRequest model)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                IUserAuthData user = _authService.GetCurrentUser();

                if (user == null)
                {
                    code = 401;
                    response = new ErrorResponse("You must be logged in.");
                    return StatusCode(code, response);
                }

                // 1. Create local discount first.
                // Your SQL insert should now resolve ShopifyProductId / ShopifyVariantId
                // from the selected PartId.
                int id = _service.Add(model, user.Id);

                // 2. Reload the full discount from SQL.
                // This is important because GetById should now include ShopifyProductId
                // and ShopifyVariantId.
                AdminDiscountCode discount = _service.GetById(id);

                // 3. Try creating the real Shopify discount.
                try
                {
                    ShopifyDiscountCreateResult shopifyResult =
                        await _shopifyAdminService.CreateBasicDiscountCodeAsync(discount);

                    _service.MarkShopifyCreated(id, new AdminDiscountCodeShopifyCreatedRequest
                    {
                        ShopifyDiscountGid = shopifyResult.DiscountGid
                    });

                    Logger.LogInformation(
                        "Shopify discount created for AdminDiscountCodeId {DiscountId}. ShopifyDiscountGid: {ShopifyDiscountGid}",
                        id,
                        shopifyResult.DiscountGid);
                }
                catch (Exception shopifyEx)
                {
                    Logger.LogError(
                        shopifyEx,
                        "Shopify discount sync failed for AdminDiscountCodeId {DiscountId}",
                        id);

                    _service.MarkError(id, shopifyEx.Message);

                    // Local row was created, but Shopify failed.
                    // We still return the local discount ID so admin can see the Error row.
                }

                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex, "Admin discount create failed.");
            }

            return StatusCode(code, response);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<AdminDiscountCode>> GetById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                AdminDiscountCode discount = _service.GetById(id);

                if (discount == null)
                {
                    code = 404;
                    response = new ErrorResponse("Discount code not found.");
                }
                else
                {
                    response = new ItemResponse<AdminDiscountCode> { Item = discount };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("paginate")]
        public ActionResult<ItemResponse<Paged<AdminDiscountCode>>> GetPaginated(
            int pageIndex,
            int pageSize,
            [FromQuery] AdminDiscountCodeSearchRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<AdminDiscountCode> paged = _service.GetPaginated(pageIndex, pageSize, model);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("No discount codes found.");
                }
                else
                {
                    response = new ItemResponse<Paged<AdminDiscountCode>> { Item = paged };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpPut("{id:int}/shopify-created")]
        public ActionResult<SuccessResponse> MarkShopifyCreated(int id, AdminDiscountCodeShopifyCreatedRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.MarkShopifyCreated(id, model);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpPut("{id:int}/deactivate")]
        public async Task<ActionResult<SuccessResponse>> Deactivate(int id, AdminDiscountCodeDeactivateRequest model)
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

                AdminDiscountCode? discount = _service.GetById(id);

                if (discount == null)
                {
                    code = 404;
                    response = new ErrorResponse("Discount code not found.");
                    return StatusCode(code, response);
                }

                // If it exists in Shopify, deactivate it there first.
                // This prevents local saying Deactivated while Shopify is still Active.
                if (!string.IsNullOrWhiteSpace(discount.ShopifyDiscountGid))
                {
                    ShopifyDiscountDeactivateResult shopifyResult =
                        await _shopifyAdminService.DeactivateDiscountCodeAsync(discount.ShopifyDiscountGid);

                    Logger.LogInformation(
                        "Shopify discount deactivated for AdminDiscountCodeId {DiscountId}. ShopifyDiscountGid: {ShopifyDiscountGid}, ShopifyStatus: {ShopifyStatus}",
                        id,
                        discount.ShopifyDiscountGid,
                        shopifyResult.Status);
                }

                // Then mark local row deactivated.
                _service.Deactivate(id, model, user.Id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex, "Admin discount deactivate failed for DiscountId {DiscountId}", id);
            }

            return StatusCode(code, response);
        }
    }
}

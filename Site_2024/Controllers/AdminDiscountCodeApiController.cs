using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/admin/discounts")]
    [ApiController]
    [Authorize(Policy = "AdminAction")]
    public class AdminDiscountCodesApiController : BaseApiController
    {
        private readonly IAdminDiscountCodeService _service;
        private readonly IAuthenticationService<IUserAuthData> _authService;

        public AdminDiscountCodesApiController(
            IAdminDiscountCodeService service,
            IAuthenticationService<IUserAuthData> authService,
            ILogger<AdminDiscountCodesApiController> logger) : base(logger)
        {
            _service = service;
            _authService = authService;
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(AdminDiscountCodeAddRequest model)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                IUserAuthData user = _authService.GetCurrentUser();
                int id = _service.Add(model, user.Id);

                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
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
        public ActionResult<SuccessResponse> Deactivate(int id, AdminDiscountCodeDeactivateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                IUserAuthData user = _authService.GetCurrentUser();
                _service.Deactivate(id, model, user.Id);

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
    }
}

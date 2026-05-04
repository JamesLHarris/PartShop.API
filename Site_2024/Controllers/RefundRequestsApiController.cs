using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Models;
using Site_2024.Models.Domain.RefundRequests;
using Site_2024.Models.Requests.RefundRequests;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Controllers;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/refunds")]
    [ApiController]
    public class RefundRequestsApiController : BaseApiController
    {
        private readonly IRefundRequestService _service;
        private readonly IAuthenticationService<IUserAuthData> _authService;

        public RefundRequestsApiController(
            IRefundRequestService service,
            IAuthenticationService<IUserAuthData> authService,
            ILogger<RefundRequestsApiController> logger) : base(logger)
        {
            _service = service;
            _authService = authService;
        }

        [HttpPost]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Create(RefundRequestAddRequest model)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                var userId = _authService.GetCurrentUser();
                int id = _service.Add(model, userId.Id);

                response = new ItemResponse<int> { Item = id };
            }
            catch (System.Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<RefundRequest>> GetById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                RefundRequest refundRequest = _service.GetById(id);

                if (refundRequest == null)
                {
                    code = 404;
                    response = new ErrorResponse("Refund request not found.");
                }
                else
                {
                    response = new ItemResponse<RefundRequest> { Item = refundRequest };
                }
            }
            catch (System.Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("paginate")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<RefundRequest>>> GetPaginated(
            int pageIndex,
            int pageSize,
            [FromQuery] RefundRequestSearchRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<RefundRequest> paged = _service.GetPaginated(pageIndex, pageSize, model);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("No refund requests found.");
                }
                else
                {
                    response = new ItemResponse<Paged<RefundRequest>> { Item = paged };
                }
            }
            catch (System.Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<SuccessResponse> UpdateStatus(int id, RefundRequestUpdateStatusRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                var userId = _authService.GetCurrentUser();
                _service.UpdateStatus(id, model, userId.Id);

                response = new SuccessResponse();
            }
            catch (System.Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }
    }
}
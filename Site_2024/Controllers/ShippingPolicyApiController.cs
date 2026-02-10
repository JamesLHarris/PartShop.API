using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests.ShippingPolicies;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;
using System.Collections.Generic;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/shippingpolicies")]
    [ApiController]
    public class ShippingPoliciesApiController : BaseApiController
    {
        private readonly IShippingPoliciesService _service;
        private readonly IAuthenticationService<IUserAuthData> _authService;

        public ShippingPoliciesApiController(IShippingPoliciesService service, IAuthenticationService<IUserAuthData> authService, ILogger<ShippingPoliciesApiController> logger)
            : base(logger)
        {
            _service = service;
            _authService = authService;
        }

        [HttpGet("all")]
        [AllowAnonymous] // or [Authorize] if you want it locked down
        public ActionResult<ItemResponse<List<ShippingPolicy>>> GetAll()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<ShippingPolicy> list = _service.GetAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No shipping policies found.");
                }
                else
                {
                    response = new ItemResponse<List<ShippingPolicy>> { Item = list };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpPost]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Create(ShippingPolicyAddRequest model)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                var user = _authService.GetCurrentUser();
                int id = _service.Add(model, user.Id);

                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }
    }
}


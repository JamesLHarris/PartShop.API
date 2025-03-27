using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginApiController : BaseApiController
    {
        private IUserService _service;
        private ILogger _logger;
        private IAuthenticationService<IUserAuthData> _authService;

        public LoginApiController(IUserService service
            ,IAuthenticationService<IUserAuthData> authService
            ,ILogger<ModelApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult<ItemResponse<int>> AddUser(UserRegisterRequest model)
        {
            int code = 200;
            ObjectResult result = null;

            try
            {
                int id = _service.Create(model);

                ItemResponse<int> response = new ItemResponse<int>() { Item = id };

                result = Created201(response);

            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex.ToString());

                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);
            }
            return result;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest model)
        {
            bool isLoggedIn = await _service.LogInAsync(model.Email, model.Password);
            if (!isLoggedIn) return Unauthorized("Invalid credentials.");
            return Ok("Login successful.");
        }

        [HttpGet("logout")]
        public async Task<ActionResult<SuccessResponse>> Logout()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                bool isLoggedIn = _authService.IsLoggedIn();

                if (isLoggedIn)
                {
                    await _authService.LogOutAsync();
                    response = new SuccessResponse();
                }
                else
                {
                    code = 400;
                    response = new ErrorResponse("App resource not found.");

                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

    }
}

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
            ,ILogger<LoginApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public ActionResult<ItemResponse<int>> Register([FromBody] UserRegisterRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                int id = _service.Create(model);
                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse>> Login([FromBody] UserLoginRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                bool success = await _service.LogInAsync(model.Email, model.Password);

                if (!success)
                {
                    code = 401;
                    response = new ErrorResponse("Invalid email or password.");
                }
                else
                {
                    int userId = _service.GetUserIdByEmail(model.Email); // <== make sure this exists
                    response = new ItemResponse<int> { Item = userId };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("me")]
        public ActionResult<IUserAuthData> GetCurrentUser()
        {
            var user = _authService.GetCurrentUser();

            if (user == null)
            {
                return Unauthorized("Not logged in or cookie not read");
            }

            return Ok(user);
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

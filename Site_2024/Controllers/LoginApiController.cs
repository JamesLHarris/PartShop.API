using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;
using System.Threading.Tasks;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginApiController : BaseApiController
    {
        private readonly IUserService _service;
        private readonly IAuthenticationService<IUserAuthData> _authService;

        public LoginApiController(
            IUserService service,
            IAuthenticationService<IUserAuthData> authService,
            ILogger<LoginApiController> logger) : base(logger)
        {
            _service = service;
            _authService = authService;
        }

        [HttpPost("register")]
        public ActionResult<ItemResponse<int>> Register([FromBody] UserRegisterRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.Create(model);
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

        [HttpPost("login")]
        public async Task<ActionResult<ItemResponse<int>>> Login([FromBody] UserLoginRequest model)
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
                    // Assumes this exists and is stable in your service layer
                    int userId = _service.GetUserIdByEmail(model.Email);
                    response = new ItemResponse<int> { Item = userId };
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

        // GET api/login/me
        [HttpGet("me")]
        public ActionResult<ItemResponse<IUserAuthData>> GetCurrentUser()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                IUserAuthData user = _authService.GetCurrentUser();

                if (user == null)
                {
                    code = 401;
                    response = new ErrorResponse("Not logged in.");
                }
                else
                {
                    response = new ItemResponse<IUserAuthData> { Item = user };
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

        // GET api/login/current?email=test@test.com
        [HttpGet("current")]
        public ActionResult<ItemResponse<User>> GetUserByEmail([FromQuery] string email)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                User user = _service.GetUserByEmail(email);

                if (user == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<User> { Item = user };
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

        // Keeping your route as-is (Week 1: don't break anything)
        [HttpGet("logout")]
        public async Task<ActionResult<SuccessResponse>> Logout()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                bool isLoggedIn = _authService.IsLoggedIn();

                if (!isLoggedIn)
                {
                    code = 401;
                    response = new ErrorResponse("Not logged in.");
                }
                else
                {
                    await _authService.LogOutAsync();
                    response = new SuccessResponse();
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
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/make")]
    [ApiController]
    public class MakesApiController : BaseApiController
    {
        private IMakeService _service;
        private ILogger _logger;

        public MakesApiController(IMakeService service
        , ILogger<MakesApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("new-make")]
        public ActionResult<ItemResponse<int>> Add(MakeAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddMake(model);

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

        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(MakeUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdateMake(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpGet("available")]
        public ActionResult<ItemResponse<List<Make>>> GetMakesAll()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Make> pages = _service.GetMakesAll();

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<Make>> { Item = pages };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpGet("companies")]
        public ActionResult<ItemResponse<List<Make>>> GetMakesAllCompanies()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Make> pages = _service.GetMakesAllCompanies();

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<Make>> { Item = pages };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpGet("available/{id:int}")]
        public ActionResult<ItemResponse<Make>> GetMakeById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Make course = _service.GetMakeById(id);

                if (course == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Make> { Item = course };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");

            }
            return StatusCode(code, response);
        }

        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.DeleteMake(id);

                response = new SuccessResponse();
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/catagory")]
    [ApiController]
    public class CatagoryApiController : BaseApiController
    {
        private ICatagoryService _service;
        private ILogger _logger;

        public CatagoryApiController(ICatagoryService service
        , ILogger<CatagoryApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("new-catagory")]
        public ActionResult<ItemResponse<int>> Add(CatagoryAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddCatagory(model);

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
        public ActionResult<SuccessResponse> Update(CatagoryUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdateCatagory(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpGet("all")]
        public ActionResult<ItemResponse<List<Catagory>>> GetAll()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Catagory> pages = _service.GetCatagoryAll();

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<Catagory>> { Item = pages };
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

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<Catagory>> GetById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Catagory course = _service.GetCatagoryById(id);

                if (course == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Catagory> { Item = course };
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
                _service.DeleteCatagory(id);

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

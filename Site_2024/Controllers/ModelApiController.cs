using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/models")]
    [ApiController]
    public class ModelApiController : BaseApiController
    {
        private IModelService _service;
        private ILogger _logger;

        public ModelApiController(IModelService service
        , ILogger<ModelApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("new-model")]
        public ActionResult<ItemResponse<int>> Add(ModelAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddModel(model);

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
        public ActionResult<SuccessResponse> Update(ModelUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdateModel(model);

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
        public ActionResult<ItemResponse<List<Model>>> GetModelsAll()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Model> pages = _service.GetModelsAll();

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<Model>> { Item = pages };
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
        public ActionResult<ItemResponse<Model>> GetModelById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Model course = _service.GetModelById(id);

                if (course == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Model> { Item = course };
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
        [HttpGet("make/{id:int}")]
        public ActionResult<ItemResponse<List<Model>>> GetByMakeId(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Model> list = _service.GetByMakeId(id);

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No models found for this make.");
                }
                else
                {
                    response = new ItemResponse<List<Model>> { Item = list };
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

        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.DeleteModel(id);

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

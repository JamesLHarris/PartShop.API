using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/locations")]
    [ApiController]
    public class LocationApiController : BaseApiController
    {
        private ILocationService _service;
        private ILogger _logger;

        public LocationApiController(ILocationService service
        , ILogger<LocationApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("new-location")]
        public ActionResult<ItemResponse<int>> Add(LocationAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddLocation(model);

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
        [HttpPost("new-site")]
        public ActionResult<ItemResponse<int>> Add(SiteAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddSite(model);

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
        [HttpPost("new-area")]
        public ActionResult<ItemResponse<int>> Add(AreaAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddArea(model);

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
        [HttpPost("new-aisle")]
        public ActionResult<ItemResponse<int>> Aisle(AisleAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddAisle(model);

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
        [HttpPost("new-section")]
        public ActionResult<ItemResponse<int>> Section(SectionAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddSection(model);

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
        [HttpPost("new-box")]
        public ActionResult<ItemResponse<int>> Add(BoxAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddBox(model);

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
        public ActionResult<SuccessResponse> Update(LocationUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdateLocation(model);

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
        public ActionResult<ItemResponse<List<Location>>> GetLocationsAll()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Location> pages = _service.GetLocationsAll();

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<Location>> { Item = pages };
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


        [HttpGet("single/{id:int}")]
        public ActionResult<ItemResponse<Location>> GetLocationById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Location course = _service.GetLocationById(id);

                if (course == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Location> { Item = course };
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
                _service.DeleteLocation(id);

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

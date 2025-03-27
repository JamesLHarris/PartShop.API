using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class PartsApiController : BaseApiController
    {

        private IPartService _service;
        private ILogger _logger;

        public PartsApiController(IPartService service
         , ILogger<PartsApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("add-new")]
        public ActionResult<ItemResponse<int>> Add(PartAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddPart(model);

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
        public ActionResult<SuccessResponse> Update(PartUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdatePart(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpPatch("location/{id:int}")]
        public ActionResult<SuccessResponse> UpdateLocation(PartLocationUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdatePartLocation(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpGet("available/admin")]
        public ActionResult<ItemResponse<Paged<Part>>> GetAvailablePaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            int availableId = 1;

            try
            {
                Paged<Part> pages = _service.GetAvailablePaginated(pageIndex, pageSize, availableId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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
        [HttpGet("available")]
        public ActionResult<ItemResponse<Paged<Part>>> GetAvailablePaginatedCustomers(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            int availableId = 1;

            try
            {
                Paged<Part> pages = _service.GetAvailablePaginatedForCustomer(pageIndex, pageSize, availableId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        [HttpGet("stock")]
        public ActionResult<ItemResponse<Paged<Part>>> GetPartsPaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<Part> pages = _service.GetPartsPaginated(pageIndex, pageSize);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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
        public ActionResult<ItemResponse<Part>> GetPartsById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Part course = _service.GetPartById(id);

                if (course == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Part> { Item = course };
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
                _service.DeletePart(id);

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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;
using System.Collections.Generic;

namespace Site_2024.Web.Api.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationApiController : BaseApiController
    {
        private readonly ILocationService _service;

        public LocationApiController(ILocationService service, ILogger<LocationApiController> logger) : base(logger)
        {
            _service = service;
        }

        [HttpPost("new-location")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Add([FromBody] LocationAddRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.AddLocation(model);
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

        // Keep route as-is, but bind id explicitly so route + model can't drift
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<SuccessResponse> Update(int id, [FromBody] LocationUpdateRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                // Ensure route id is the source of truth
                model.Id = id;

                _service.UpdateLocation(model);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Location>>> GetLocationsAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Location> list = _service.GetLocationsAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<Location>> { Item = list };
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

        [HttpGet("{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Location>> GetLocationById(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Location location = _service.GetLocationById(id);

                if (location == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Location> { Item = location };
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

        [HttpGet("hierarchy/{siteId:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Location>>> GetHierarchy(int siteId)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Location> list = _service.GetHierarchy(siteId);

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No location hierarchy found.");
                }
                else
                {
                    response = new ItemResponse<List<Location>> { Item = list };
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

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                _service.DeleteLocation(id);
                response = new SuccessResponse();
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


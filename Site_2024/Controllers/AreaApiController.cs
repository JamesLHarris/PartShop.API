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
    [Route("api/areas")]
    public class AreaApiController : BaseApiController
    {
        private readonly IAreaService _service;

        public AreaApiController(IAreaService service, ILogger<AreaApiController> logger) : base(logger)
        {
            _service = service;
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Area>>> GetAreasAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Area> list = _service.GetAreasAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No areas found.");
                }
                else
                {
                    response = new ItemResponse<List<Area>> { Item = list };
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

        // Keep existing route so nothing breaks:
        [HttpGet("area/{id:int}")]
        // Add clear alias route (safe Week-1 improvement):
        [HttpGet("site/{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Area>>> GetBySiteId(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Area> list = _service.GetAreaBySiteId(id);

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<List<Area>> { Item = list };
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

        [HttpPost("new-area")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Create([FromBody] AreaAddRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.AddArea(model);
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



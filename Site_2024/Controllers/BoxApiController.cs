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
    [Route("api/boxes")]
    public class BoxApiController : BaseApiController
    {
        private readonly IBoxService _service;

        public BoxApiController(IBoxService service, ILogger<BoxApiController> logger) : base(logger)
        {
            _service = service;
        }

        [HttpGet("all")]
        public ActionResult<ItemResponse<List<Box>>> GetBoxAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Box> list = _service.GetBoxAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No boxes found.");
                }
                else
                {
                    response = new ItemResponse<List<Box>> { Item = list };
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

        // Keep route as-is to avoid breaking frontend
        [HttpGet("box/{id:int}")]
        public ActionResult<ItemResponse<List<Box>>> GetBoxBySectionId(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Box> list = _service.GetBoxBySectionId(id);

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<List<Box>> { Item = list };
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

        [HttpPost("new-box")]
        public ActionResult<ItemResponse<int>> Add([FromBody] BoxAddRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.AddBox(model);
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

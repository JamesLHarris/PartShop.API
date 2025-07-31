using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Services;
using Site_2024.Web.Api.Responses;
using System;
using System.Collections.Generic;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Controllers
{
    [ApiController]
    [Route("api/boxes")]
    public class BoxApiController : BaseApiController
    {
        private readonly IBoxService _service;
        private ILogger _logger;

        public BoxApiController(IBoxService service
        , ILogger<BoxApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
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
                Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }
        [HttpGet("box/{id:int}")]
        public ActionResult<ItemResponse<List<Box>>> GetBoxBySectionId(int id)
        {
            int code = 200;
            BaseResponse response = null;

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
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(code, response);
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
    }
}


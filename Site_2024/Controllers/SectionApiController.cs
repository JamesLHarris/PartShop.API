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
    [Route("api/sections")]
    public class SectionApiController : BaseApiController
    {
        private readonly ISectionService _service;
        private ILogger _logger;

        public SectionApiController(ISectionService service
        , ILogger<SectionApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("all")]
        public ActionResult<ItemResponse<List<Section>>> GetSectionfAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Section> list = _service.GetSectionAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No sections found.");
                }
                else
                {
                    response = new ItemResponse<List<Section>> { Item = list };
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

        [HttpGet("section/{id:int}")]
        public ActionResult<ItemResponse<List<Section>>> GetSectionByShelfId(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Section> list = _service.GetSectionByShelfId(id);

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<List<Section>> { Item = list };
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
    }
}

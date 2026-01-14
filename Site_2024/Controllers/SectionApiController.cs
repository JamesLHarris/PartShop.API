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
    [Route("api/sections")]
    public class SectionApiController : BaseApiController
    {
        private readonly ISectionService _service;

        public SectionApiController(ISectionService service, ILogger<SectionApiController> logger) : base(logger)
        {
            _service = service;
        }

        // GET api/sections/all
        [HttpGet("all")]
        public ActionResult<ItemResponse<List<Section>>> GetSectionAll()
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
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        // Drill-down: Sections by ShelfId
        // GET api/sections/section/5  (where 5 is a shelfId)
        [HttpGet("section/{id:int}")]
        public ActionResult<ItemResponse<List<Section>>> GetSectionByShelfId(int id)
        {
            int code = 200;
            BaseResponse response;

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
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        // POST api/sections/new-section
        [HttpPost("new-section")]
        public ActionResult<ItemResponse<int>> Add([FromBody] SectionAddRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.AddSection(model);
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

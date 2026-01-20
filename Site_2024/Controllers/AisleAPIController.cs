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
    [Route("api/aisles")]
    public class AisleApiController : BaseApiController
    {
        private readonly IAisleService _service;

        public AisleApiController(IAisleService service, ILogger<AisleApiController> logger) : base(logger)
        {
            _service = service;
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Aisle>>> GetAisleAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Aisle> list = _service.GetAisleAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No aisles found.");
                }
                else
                {
                    response = new ItemResponse<List<Aisle>> { Item = list };
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

        [HttpGet("aisle/{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Aisle>>> GetAisleByAreaId(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Aisle> list = _service.GetAisleByAreaId(id);

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<List<Aisle>> { Item = list };
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

        [HttpPost("new-aisle")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> CreateAisle([FromBody] AisleAddRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.AddAisle(model);
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


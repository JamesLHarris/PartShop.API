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
    [Route("api/aisles")]
    public class AisleApiController : BaseApiController
    {
        private readonly IAisleService _service;
        private ILogger _logger;

        public AisleApiController(IAisleService service
        , ILogger<AisleApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("all")]
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
                Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("aisle/{id:int}")]
        public ActionResult<ItemResponse<List<Aisle>>> GetAisleByAreaId(int id)
        {
            int code = 200;
            BaseResponse response = null;

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
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(code, response);
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
    }
}

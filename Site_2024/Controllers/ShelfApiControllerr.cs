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
    [Route("api/shelves")]
    public class ShelfApiController : BaseApiController
    {
        private readonly IShelfService _service;
        private ILogger _logger;

        public ShelfApiController(IShelfService service
        , ILogger<ShelfApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("all")]
        public ActionResult<ItemResponse<List<Shelf>>> GetShelfAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Shelf> list = _service.GetShelfAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No shelves found.");
                }
                else
                {
                    response = new ItemResponse<List<Shelf>> { Item = list };
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
        [HttpGet("shelf/{id:int}")]
        public ActionResult<ItemResponse<List<Shelf>>> GetShelfByAisleId(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Shelf> list = _service.GetShelfByAisleId(id);

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<List<Shelf>> { Item = list };
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

        [HttpPost("new-shelf")]
        public ActionResult<ItemResponse<int>> Shelf(ShelfAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddShelf(model);

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


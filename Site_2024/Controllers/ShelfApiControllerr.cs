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
    [Route("api/shelves")]
    public class ShelfApiController : BaseApiController
    {
        private readonly IShelfService _service;

        public ShelfApiController(IShelfService service, ILogger<ShelfApiController> logger) : base(logger)
        {
            _service = service;
        }

        // GET api/shelves/all
        [HttpGet("all")]
        [Authorize(Policy = "AdminAction")]
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
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        // Drill-down: Shelves by AisleId
        // GET api/shelves/shelf/5  (where 5 is an aisleId)
        [HttpGet("shelf/{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Shelf>>> GetShelfByAisleId(int id)
        {
            int code = 200;
            BaseResponse response;

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
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        // POST api/shelves/new-shelf
        [HttpPost("new-shelf")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Add([FromBody] ShelfAddRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.AddShelf(model);
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

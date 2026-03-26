using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Interfaces;
using System;
using System.Collections.Generic;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/conditions")]
    [ApiController]
    public class ConditionsApiController : BaseApiController
    {
        private readonly IConditionService _service;

        public ConditionsApiController(IConditionService service, ILogger<ConditionsApiController> logger)
            : base(logger)
        {
            _service = service;
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public ActionResult<ItemResponse<List<Condition>>> GetAll()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<Condition> list = _service.GetAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No conditions found.");
                }
                else
                {
                    response = new ItemResponse<List<Condition>> { Item = list };
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
        [AllowAnonymous]
        public ActionResult<ItemResponse<Condition>> GetById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Condition item = _service.GetById(id);

                if (item == null)
                {
                    code = 404;
                    response = new ErrorResponse("Condition not found.");
                }
                else
                {
                    response = new ItemResponse<Condition> { Item = item };
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

        [HttpPost]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Create([FromBody] string name)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                int id = _service.Add(name);
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

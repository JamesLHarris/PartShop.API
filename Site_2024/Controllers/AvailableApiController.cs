﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Services;
using Site_2024.Web.Api.Responses;
using System;
using System.Collections.Generic;

namespace Site_2024.Web.Api.Controllers
{
    [ApiController]
    [Route("api/available")]
    public class AvailableApiController : BaseApiController
    {
        private readonly IAvailableService _service;
        private readonly ILogger<AvailableApiController> _logger;

        public AvailableApiController(IAvailableService service, ILogger<AvailableApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("all")]
        public ActionResult<ItemResponse<List<Available>>> GetAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Available> list = _service.GetAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No records found.");
                }
                else
                {
                    response = new ItemResponse<List<Available>> { Item = list };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                _logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<Available>> GetById(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                var item = _service.GetById(id);

                if (item == null)
                {
                    code = 404;
                    response = new ErrorResponse("Item not found.");
                }
                else
                {
                    response = new ItemResponse<Available> { Item = item };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                _logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Add([FromBody] string status)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                int id = _service.Add(status);
                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                _logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                _service.Delete(id);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                _logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }
    }
}

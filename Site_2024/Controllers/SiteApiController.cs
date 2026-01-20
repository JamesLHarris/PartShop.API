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
    [Route("api/sites")]
    public class SiteApiController : BaseApiController
    {
        private readonly ISiteService _service;

        public SiteApiController(ISiteService service, ILogger<SiteApiController> logger) : base(logger)
        {
            _service = service;
        }

        // GET api/sites/all
        [HttpGet("all")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<Site>>> GetSitesAll()
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<Site> list = _service.GetSitesAll();

                if (list == null || list.Count == 0)
                {
                    code = 404;
                    response = new ErrorResponse("No sites found.");
                }
                else
                {
                    response = new ItemResponse<List<Site>> { Item = list };
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

        // POST api/sites/new-site
        [HttpPost("new-site")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Add([FromBody] SiteAddRequest model)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                int id = _service.AddSite(model);
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

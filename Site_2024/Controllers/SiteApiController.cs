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
    [Route("api/sites")]
    public class SiteApiController : BaseApiController
    {
        private readonly ISiteService _service;
        private ILogger _logger;

        public SiteApiController(ISiteService service
        , ILogger<SiteApiController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("all")]
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
                Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpPost("new-site")]
        public ActionResult<ItemResponse<int>> Add(SiteAddRequest model)
        {
            ObjectResult result = null;

            try
            {

                int id = _service.AddSite(model);

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

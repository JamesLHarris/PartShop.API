using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;

namespace Site_2024.Web.Api.Controllers
{
    [ApiController]
    [Route("api/partsaudit")]
    public class AuditApiController : BaseApiController
    {
        private readonly IAuditService _service;

        // Week-1: keep constructor minimal and correct the logger generic type
        public AuditApiController(IAuditService service, ILogger<AuditApiController> logger) : base(logger)
        {
            _service = service;
        }

        // FIX: remove leading "/" so it respects controller route prefix
        // GET api/partsaudit/recent?pageIndex=0&pageSize=10
        [HttpGet("recent")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<PartAudit>>> GetRecentPaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<PartAudit> page = _service.GetAuditRecentPaginated(pageIndex, pageSize);

                if (page == null)
                {
                    code = 404;
                    response = new ErrorResponse("Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<PartAudit>> { Item = page };
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

        // GET api/partsaudit/part/123?pageIndex=0&pageSize=10
        [HttpGet("part/{partId:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<PartAudit>>> GetByPartIdPaginated(int partId, int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<PartAudit> page = _service.GetAuditByPartIdPaginated(partId, pageIndex, pageSize);

                if (page == null)
                {
                    code = 404;
                    response = new ErrorResponse("Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<PartAudit>> { Item = page };
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

        // GET api/partsaudit/recent/all?maxRows=25
        [HttpGet("recent/all")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<PartAudit>> GetRecent(int maxRows)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                PartAudit item = _service.GetAuditByRecent(maxRows);

                if (item == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<PartAudit> { Item = item };
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

        // GET api/partsaudit/single/123?maxRows=25
        [HttpGet("single/{partId:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<PartAudit>> GetSingleByPartId(int partId, int maxRows)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                PartAudit item = _service.GetAuditByPartId(partId, maxRows);

                if (item == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<PartAudit> { Item = item };
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
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Models;
using Site_2024.Models.Domain.RefundRequests;
using Site_2024.Models.Requests.RefundRequests;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/refunds")]
    [ApiController]
    public class RefundRequestsApiController : BaseApiController
    {
        private readonly IRefundRequestService _service;
        private readonly IAuthenticationService<IUserAuthData> _authService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RefundRequestsApiController(
            IRefundRequestService service,
            IAuthenticationService<IUserAuthData> authService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<RefundRequestsApiController> logger) : base(logger)
        {
            _service = service;
            _authService = authService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("reasons")]
        [AllowAnonymous]
        public ActionResult<ItemResponse<List<ReturnReason>>> GetReasons()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<ReturnReason> list = _service.GetReasons();
                response = new ItemResponse<List<ReturnReason>> { Item = list };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("statuses")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<ReturnStatus>>> GetStatuses()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<ReturnStatus> list = _service.GetStatuses();
                response = new ItemResponse<List<ReturnStatus>> { Item = list };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpPost]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Create(RefundRequestAddRequest model)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                var user = _authService.GetCurrentUser();
                int id = _service.Add(model, user.Id);

                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpPost("customer-submit")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public ActionResult<ItemResponse<int>> CustomerSubmit([FromForm] RefundRequestCustomerSubmitRequest model)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                if (model == null)
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Return request payload is required."));
                }

                List<ReturnReason> reasons = _service.GetReasons();
                ReturnReason selectedReason = reasons.FirstOrDefault(r => r.Id == model.ReturnReasonId);

                if (selectedReason == null)
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Please select a valid return reason."));
                }

                if (selectedReason.RequiresNotes && string.IsNullOrWhiteSpace(model.Notes))
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("This return reason requires a written description."));
                }

                if (selectedReason.RequiresPhotos && (model.Photos == null || model.Photos.Count == 0))
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("This return reason requires at least one proof photo."));
                }

                RefundRequestAddRequest addRequest = new RefundRequestAddRequest
                {
                    PartId = model.PartId,
                    ShopifyOrderId = model.ShopifyOrderId,
                    OrderNumber = model.OrderNumber,
                    CustomerEmail = model.CustomerEmail,
                    ReturnReasonId = model.ReturnReasonId,
                    Reason = selectedReason.Name,
                    Notes = model.Notes,
                    Items = new List<RefundRequestItemAddRequest>
                    {
                        new RefundRequestItemAddRequest
                        {
                            PartId = model.PartId,
                            Quantity = 1
                        }
                    }
                };

                int id = _service.Add(addRequest, null);

                if (model.Photos != null && model.Photos.Count > 0)
                {
                    SaveCustomerPhotos(id, model.Photos);
                }

                if (id <= 0)
                {
                    throw new Exception($"Refund request insert failed. Returned Id was {id}.");
                }

                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<RefundRequest>> GetById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                RefundRequest refundRequest = _service.GetById(id);

                if (refundRequest == null)
                {
                    code = 404;
                    response = new ErrorResponse("Refund request not found.");
                }
                else
                {
                    response = new ItemResponse<RefundRequest> { Item = refundRequest };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("paginate")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<RefundRequest>>> GetPaginated(
            int pageIndex,
            int pageSize,
            [FromQuery] RefundRequestSearchRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<RefundRequest> paged = _service.GetPaginated(pageIndex, pageSize, model);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("No refund requests found.");
                }
                else
                {
                    response = new ItemResponse<Paged<RefundRequest>> { Item = paged };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<SuccessResponse> UpdateStatus(int id, RefundRequestUpdateStatusRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                var user = _authService.GetCurrentUser();
                _service.UpdateStatus(id, model, user.Id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        private void SaveCustomerPhotos(int refundRequestId, List<IFormFile> photos)
        {
            string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
            const long maxBytes = 5 * 1024 * 1024;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "returns");
            Directory.CreateDirectory(uploadsFolder);

            for (int i = 0; i < photos.Count; i++)
            {
                IFormFile photo = photos[i];

                if (photo == null || photo.Length == 0)
                {
                    throw new Exception("One of the proof photos is empty.");
                }

                string ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    throw new Exception($"Invalid proof photo type: {ext}. Allowed: jpg, jpeg, png, webp.");
                }

                if (photo.Length > maxBytes)
                {
                    throw new Exception("Proof photo too large. Max size is 5MB.");
                }

                string fileName = $"{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                _service.AddPhoto(refundRequestId, new RefundRequestPhotoAddRequest
                {
                    Url = $"/uploads/returns/{fileName}",
                    OriginalFileName = photo.FileName,
                    ContentType = photo.ContentType,
                    SortOrder = i
                });
            }
        }
    }
}

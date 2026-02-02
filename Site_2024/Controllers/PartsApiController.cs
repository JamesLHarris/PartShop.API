using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Site_2024.Models.Domain.Parts;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class PartsApiController : BaseApiController
    {
        private readonly IPartService _service;
        private readonly ILocationService _locationService;
        private readonly IAuthenticationService<IUserAuthData> _authService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PartsApiController(
            IPartService service,
            ILocationService locationService,
            ILogger<PartsApiController> logger,
            IAuthenticationService<IUserAuthData> authService,
            IWebHostEnvironment webHostEnvironment
        ) : base(logger)
        {
            _service = service;
            _locationService = locationService;
            _authService = authService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("add-new")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<int>> Add([FromForm] PartAddRequest model, IFormFile? image)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    code = 401;
                    response = new ErrorResponse("You must be logged in.");
                    return StatusCode(code, response);
                }

                // Validate LocationId exists (Location table is already leaf-level: includes boxId)
                var loc = _locationService.GetLocationById(model.LocationId);
                if (loc == null)
                {
                    code = 400;
                    response = new ErrorResponse("Invalid LocationId.");
                    return StatusCode(code, response);
                }

                // Basic file validation
                string? imageUrl = null;
                if (image != null && image.Length > 0)
                {
                    string ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                    string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };

                    if (!allowed.Contains(ext))
                    {
                        code = 400;
                        response = new ErrorResponse("Invalid image type. Allowed: jpg, jpeg, png, webp.");
                        return StatusCode(code, response);
                    }

                    // 5MB cap (adjust if you want)
                    const long maxBytes = 5 * 1024 * 1024;
                    if (image.Length > maxBytes)
                    {
                        code = 400;
                        response = new ErrorResponse("Image too large. Max size is 5MB.");
                        return StatusCode(code, response);
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = $"{Guid.NewGuid()}{ext}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    imageUrl = $"/uploads/items/{fileName}";
                }

                model.Image = imageUrl;      // can be null now (SQL fixed)
                model.AvailableId = 1;       // server rule: new parts default to Available
                int userId = user.Id;

                int id = _service.Insert(model, userId);

                response = new ItemResponse<int> { Item = id };
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        // Separate image upload endpoint (keeps Parts_Insert clean)
        [HttpPost("{id:int}/image")]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<BaseResponse> UploadImage(int id, [FromForm] Requests.Parts.PartImageUploadRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    code = 401;
                    return StatusCode(code, new ErrorResponse("You must be logged in."));
                }

                IFormFile image = model.Image;

                if (image == null || image.Length == 0)
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Image is required."));
                }

                string ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Invalid image type. Allowed: jpg, jpeg, png, webp."));
                }

                const long maxBytes = 5 * 1024 * 1024;
                if (image.Length > maxBytes)
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Image too large. Max size is 5MB."));
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = $"{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                string imageUrl = $"/uploads/items/{fileName}";

                // Critical: pass user.Id so @LastMovedBy is correct + audit is accurate
                _service.PatchPart(id, new PartPatchRequest { Image = imageUrl }, user.Id);

                response = new ItemResponse<string> { Item = imageUrl };
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }



        [HttpPatch("{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<BaseResponse> UpdatePart(int id, [FromBody] PartPatchRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    code = 401;
                    return StatusCode(code, new ErrorResponse("You must be logged in."));
                }

                if (model == null)
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Patch payload is required."));
                }

                // Normalize strings
                model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
                model.Image = string.IsNullOrWhiteSpace(model.Image) ? null : model.Image.Trim();

                // Reject empty patch (no-op)
                if (!HasAnyPatchField(model))
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("At least one field is required to patch."));
                }

                // Validate manual fields only (high ROI)
                if (model.Price.HasValue)
                {
                    if (model.Price.Value < 0.01m || model.Price.Value > 1_000_000m)
                    {
                        code = 400;
                        return StatusCode(code, new ErrorResponse("Price must be between 0.01 and 1,000,000."));
                    }

                    if (decimal.Round(model.Price.Value, 2) != model.Price.Value)
                    {
                        code = 400;
                        return StatusCode(code, new ErrorResponse("Price can have at most 2 decimal places."));
                    }
                }

                if (model.Description != null && model.Description.Length > 4000)
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Description cannot exceed 4000 characters."));
                }

                if (model.Image != null && model.Image.Length > 260)
                {
                    code = 400;
                    return StatusCode(code, new ErrorResponse("Image path cannot exceed 260 characters."));
                }

                _service.PatchPart(id, model, user.Id);
                response = new SuccessResponse();
            }
            catch (Exception ex) when (IsFkViolation(ex))
            {
                code = 400;
                response = new ErrorResponse("Invalid LocationId or AvailableId.");
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse("An unexpected error occurred.");
            }


            return StatusCode(code, response);
        }

        private static bool HasAnyPatchField(PartPatchRequest m)
        {
            return m.Price.HasValue
                || m.AvailableId.HasValue
                || m.Rusted.HasValue
                || m.Tested.HasValue
                || m.LocationId.HasValue
                || !string.IsNullOrWhiteSpace(m.Description)
                || !string.IsNullOrWhiteSpace(m.Image);
        }

        private static bool IsFkViolation(Exception ex)
        {
            // Walk the exception chain to look for SQL Server FK violation signature.
            // SQL Server FK violations often contain: "FOREIGN KEY constraint" and/or "conflicted with the FOREIGN KEY constraint"
            // If your data layer wraps exceptions, this still works.
            Exception current = ex;
            while (current != null)
            {
                string msg = current.Message ?? string.Empty;
                if (msg.IndexOf("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    msg.IndexOf("conflicted with the FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }


        [HttpGet("available/admin")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<Part>>> GetAvailablePaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response;
            int availableId = 1;

            try
            {
                Paged<Part> pages = _service.GetAvailablePaginated(pageIndex, pageSize, availableId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        [HttpGet("model/{modelId:int}")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByModelCustomer(int pageIndex, int pageSize, int modelId)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<Part> pages = _service.GetByModelPaginatedCustomer(pageIndex, pageSize, modelId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("Parts not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        [HttpGet("model/{modelId:int}/admin")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByModelAdmin(int pageIndex, int pageSize, int modelId)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<Part> pages = _service.GetByModelPaginated(pageIndex, pageSize, modelId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("Parts not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        [HttpGet("category/{categoryId:int}")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByCategoryCustomer(int pageIndex, int pageSize, int categoryId)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<Part> pages = _service.GetByCategoryPaginatedCustomer(pageIndex, pageSize, categoryId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("Parts not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        [HttpGet("category/{categoryId:int}/admin")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByCategoryAdmin(int pageIndex, int pageSize, int categoryId)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<Part> pages = _service.GetByCategoryPaginated(pageIndex, pageSize, categoryId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("Parts not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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
        //New API call for paginated parts
        [HttpGet("/api/parts/customer/paginate")]
        public ActionResult<ItemResponse<Paged<Part>>> GetCustomerPaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                const int availableId = 1;
                Paged<Part> pages = _service.GetAvailablePaginatedForCustomer(pageIndex, pageSize, availableId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("Parts not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        //Old API call for paginated parts DO NOT DELETE YET
        [HttpGet("available")]
        public ActionResult<ItemResponse<Paged<Part>>> GetAvailablePaginatedCustomers(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response;
            int availableId = 1;

            try
            {
                Paged<Part> pages = _service.GetAvailablePaginatedForCustomer(pageIndex, pageSize, availableId);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        [HttpGet("stock")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Paged<Part>>> GetPartsPaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<Part> pages = _service.GetPartsPaginated(pageIndex, pageSize);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Part>> { Item = pages };
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

        [HttpGet("admin/{id:int}")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<Part>> GetPartsById(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Part part = _service.GetPartById(id);

                if (part == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Part> { Item = part };
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

        [HttpGet("part/{id:int}")]
        public ActionResult<ItemResponse<Part>> GetPartByIdCustomer(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Part part = _service.GetPartByIdCustomer(id);

                if (part == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Part> { Item = part };
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

        [HttpGet("search")]
        [Authorize(Policy = "AdminAction")]
        public ActionResult<ItemResponse<List<PartSearchResult>>> Search([FromQuery] PartSearchRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                List<PartSearchResult> items = _service.Search(model) ?? new List<PartSearchResult>();
                response = new ItemResponse<List<PartSearchResult>> { Item = items };
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("search/customer")]
        public ActionResult<ItemResponse<Paged<PartCustomer>>> SearchCustomer(int pageIndex, int pageSize, [FromQuery] CustomerSearchRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                Paged<PartCustomer> pages = _service.SearchCustomer(pageIndex, pageSize, model);

                if (pages == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<PartCustomer>> { Item = pages };
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

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "PartsDelete")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                _service.DeletePart(id);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        // Keep this as a debug endpoint if you still need it (Week 1: stability).
        [HttpGet("test-image")]
        public IActionResult GetTestImage()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "items",
                "963835b6-bb43-494b-83b4-0102f3d6a86b.jpg");

            if (!System.IO.File.Exists(path))
            {
                return NotFound("File not found at: " + path);
            }

            try
            {
                return PhysicalFile(path, "image/jpeg");
            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex.ToString());
                return StatusCode(500, $"Error sending file: {ex.Message}");
            }
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class PartsApiController : BaseApiController
    {
        private readonly IPartService _service;
        private readonly IAuthenticationService<IUserAuthData> _authService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PartsApiController(
            IPartService service,
            ILogger<PartsApiController> logger,
            IAuthenticationService<IUserAuthData> authService,
            IWebHostEnvironment webHostEnvironment
        ) : base(logger)
        {
            _service = service;
            _authService = authService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("add-new")]
        [Authorize]
        public ActionResult<ItemResponse<int>> Add([FromForm] PartAddRequest model, IFormFile? image)
        {
            int code = 201;
            BaseResponse response;

            try
            {
                string? imageUrl = null;

                if (image != null && image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    base.Logger.LogInformation("WebRootPath: {Path}", _webHostEnvironment.WebRootPath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    imageUrl = $"/uploads/items/{fileName}";
                }

                model.Image = imageUrl;
                model.AvailableId = 1;

                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    code = 401;
                    response = new ErrorResponse("You must be logged in.");
                    return StatusCode(code, response);
                }

                int id = _service.Insert(model, user.Id);
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

        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(int id, [FromBody] PartUpdateRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                // Route id is source of truth (prevents drift)
                model.Id = id;

                _service.UpdatePart(model);
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

        [HttpPatch("location/{id:int}")]
        public ActionResult<SuccessResponse> UpdateLocation(int id, [FromBody] PartLocationUpdateRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                // Route id is source of truth (prevents drift)
                model.Id = id;

                _service.UpdatePartLocation(model);
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

        [HttpPatch("{id:int}")]
        public ActionResult<BaseResponse> UpdatePart(int id, [FromBody] PartPatchRequest model)
        {
            int code = 200;
            BaseResponse response;

            try
            {
                _service.PatchPart(id, model);
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

        [HttpGet("available/admin")]
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

        [HttpDelete("{id:int}")]
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


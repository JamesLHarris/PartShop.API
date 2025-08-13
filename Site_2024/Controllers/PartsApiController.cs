using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class PartsApiController : BaseApiController
    {

        private IPartService _service;
        private ILogger _logger;
        private IAuthenticationService<IUserAuthData> _authService;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public PartsApiController(
            IPartService service,
            ILogger<PartsApiController> logger,
            IAuthenticationService<IUserAuthData> authService,
            IWebHostEnvironment webHostEnvironment
        ) : base(logger)
        {
            _service = service;
            _logger = logger;
            _authService = authService;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpPost("add-new")]
        [Authorize]
        public ActionResult<ItemResponse<int>> Add([FromForm] PartAddRequest model, IFormFile? image)
        {
            try
            {
                string imageUrl = null;

                if (image != null && image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items");
                    Directory.CreateDirectory(uploadsFolder); // ensure directory exists

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    Console.WriteLine($"WebRootPath: {_webHostEnvironment.WebRootPath}");
                    _logger.LogInformation("WebRootPath: {Path}", _webHostEnvironment.WebRootPath);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    imageUrl = $"/uploads/items/{fileName}";
                }

                model.Image = imageUrl;
                model.AvailableId = 1;
                var user = _authService.GetCurrentUser();
                if (user == null) return Unauthorized("You must be logged in.");

                int userId = user.Id;
                //if (!user.IsAdmin)
                //{
                //    return Unauthorized("Admin privileges required.");
                //}
                int id = _service.Insert(model, userId);

                return Created201(new ItemResponse<int> { Item = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }


        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(PartUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdatePart(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpPatch("location/{id:int}")]
        public ActionResult<SuccessResponse> UpdateLocation(PartLocationUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                //int currentUserId = _authService.GetCurrentUserId();
                //currentUserId   <----- THIS WILL BE USED FOR A LATER UPDATE 

                _service.UpdatePartLocation(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpGet("available/admin")]
        public ActionResult<ItemResponse<Paged<Part>>> GetAvailablePaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
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
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }
        [HttpGet("model/{modelId:int}")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByModelCustomer(int pageIndex, int pageSize, int modelId)
        {
            int code = 200;
            BaseResponse response = null;

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
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("model/{modelId:int}/admin")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByModelAdmin(int pageIndex, int pageSize, int modelId)
        {
            int code = 200;
            BaseResponse response = null;

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
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("category/{categoryId:int}")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByCategoryCustomer(int pageIndex, int pageSize, int categoryId)
        {
            int code = 200;
            BaseResponse response = null;

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
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("category/{categoryId:int}/admin")]
        public ActionResult<ItemResponse<Paged<Part>>> GetByCategoryAdmin(int pageIndex, int pageSize, int categoryId)
        {
            int code = 200;
            BaseResponse response = null;

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
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }


        [HttpGet("available")]
        public ActionResult<ItemResponse<Paged<Part>>> GetAvailablePaginatedCustomers(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
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
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpGet("stock")]
        public ActionResult<ItemResponse<Paged<Part>>> GetPartsPaginated(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;

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
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpGet("available/{id:int}")]
        public ActionResult<ItemResponse<Part>> GetPartsById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Part course = _service.GetPartById(id);

                if (course == null)
                {
                    code = 404;
                    response = new ErrorResponse("Not found.");
                }
                else
                {
                    response = new ItemResponse<Part> { Item = course };
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

        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.DeletePart(id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;

                response = new ErrorResponse(ex.Message);

            }

            return StatusCode(code, response);
        }

        [HttpGet("test-image")]
        public IActionResult GetTestImage()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "items", "963835b6-bb43-494b-83b4-0102f3d6a86b.jpg");

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
                return StatusCode(500, $"Error sending file: {ex.Message}");
            }
        }



    }
}

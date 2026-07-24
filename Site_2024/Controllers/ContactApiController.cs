using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Responses;
using Site_2024.Web.Api.Services;

namespace Site_2024.Web.Api.Controllers
{
    [Route("api/contact")]
    [ApiController]
    public class ContactApiController : BaseApiController
    {
        private readonly ISmtpEmailService _emailService;
        private readonly IPartService _partService;

        public ContactApiController(
            ISmtpEmailService emailService,
            IPartService partService,
            ILogger<ContactApiController> logger) : base(logger)
        {
            _emailService = emailService;
            _partService = partService;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult<SuccessResponse> Send(ContactEmailRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Part part = null;

                if (model.PartId.HasValue)
                {
                    part = _partService.GetPartByIdCustomer(model.PartId.Value);

                    if (part == null)
                    {
                        return NotFound(new ErrorResponse("The referenced part could not be found."));
                    }
                }

                string requestOrigin = Request.Headers.Origin.FirstOrDefault();

                _emailService.SendContactEmail(model, part, requestOrigin);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse("Unable to send contact message at this time.");
                Logger.LogError(ex, "Contact form email failed.");
            }

            return StatusCode(code, response);
        }
    }
}

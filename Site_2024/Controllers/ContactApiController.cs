using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ContactApiController(
            ISmtpEmailService emailService,
            ILogger<ContactApiController> logger) : base(logger)
        {
            _emailService = emailService;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult<SuccessResponse> Send(ContactEmailRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _emailService.SendContactEmail(model);
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

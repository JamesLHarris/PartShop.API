using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests.Parts
{
    public class PartImageUploadRequest
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}

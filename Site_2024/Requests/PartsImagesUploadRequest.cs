using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class PartImagesUploadRequest
    {
        [Required]
        public List<IFormFile> Images { get; set; } = new();
    }
}

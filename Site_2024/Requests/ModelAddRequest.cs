using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class ModelAddRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string Name { get; set; }
    }
}

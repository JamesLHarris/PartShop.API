using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class AvailableAddRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Status { get; set; }
    }
}

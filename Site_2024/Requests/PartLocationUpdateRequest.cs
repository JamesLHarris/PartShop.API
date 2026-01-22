using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class PartLocationUpdateRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int LocationId { get; set; }
    }
}

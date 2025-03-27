using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class PartUpdateRequest : PartAddRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
    }
}

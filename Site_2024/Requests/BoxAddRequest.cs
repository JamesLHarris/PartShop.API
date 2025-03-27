using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class BoxAddRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
    }
}

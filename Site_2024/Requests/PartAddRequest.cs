using Site_2024.Web.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class PartAddRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        public int CatagoryId { get; set; }
        [Required]
        public int MakeId { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string PartNumber { get; set; }

        public bool Rusted { get; set; }
        public bool Tested { get; set; }
        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int LocationId { get; set; }

        [StringLength(500, MinimumLength = 2)]
        public string? Image { get; set; }
        [Required]
        public int AvailableId { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}

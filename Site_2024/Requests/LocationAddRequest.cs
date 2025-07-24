using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class LocationAddRequest
    {
        [Required]
        public int SiteId { get; set; }
        [Required]
        public int AreaId { get; set; }
        [Required]
        public int AisleId { get; set; }
        [Required]

        public int ShelfId { get; set; }
        [Required]
        public int SectionId { get; set; }
        [Required]
        public int BoxId { get; set; }
    }
}

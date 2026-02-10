using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class PartAddRequest
    {
        [Required, StringLength(128, MinimumLength = 2)]
        public string Name { get; set; }

        [Range(1, int.MaxValue)]
        public int CatagoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int MakeId { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required, StringLength(128, MinimumLength = 2)]
        public string PartNumber { get; set; }

        public bool Rusted { get; set; }
        public bool Tested { get; set; }

        [Required, StringLength(4000, MinimumLength = 2)]
        public string Description { get; set; }

        [Range(typeof(decimal), "0.01", "99999999.99")]
        public decimal Price { get; set; }

        // Inventory quantity (admin-controlled)
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int LocationId { get; set; }

        // server-assigned
        public string? Image { get; set; }
        public int AvailableId { get; set; }
        public int UserId { get; set; }
    }
}

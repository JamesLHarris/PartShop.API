using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class AdminDiscountCodeDeactivateRequest
    {
        [StringLength(2000)]
        public string? AdminNotes { get; set; }
    }
}

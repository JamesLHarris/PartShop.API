using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class PartCategoryAddRequest
    {
        [Range(1, int.MaxValue)]
        public int CatagoryId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Models
{
    public class DbCategory : EntityBase
    {
        public Int64 Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}

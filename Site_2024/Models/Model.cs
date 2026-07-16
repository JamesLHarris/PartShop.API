namespace Site_2024.Web.Api.Models
{
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // These values are populated by Model_GetByMakeId.
        // MakeId is the actual dbo.Make row for this company/model pair.
        public int? MakeId { get; set; }

        // CompanyMakeId is the representative Make.Id used by the company dropdown.
        public int? CompanyMakeId { get; set; }

        public string Company { get; set; }
    }
}

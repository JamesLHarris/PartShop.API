namespace Site_2024.Web.Api.Requests
{
    public class PartSearchRequest
    {
        public string? q { get; set; }
        public int? CatagoryId { get; set; }
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }
        public int? YearMin { get; set; }
        public int? YearMax { get; set; }
        public int? AvailableId { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool? Rusted { get; set; }
        public bool? Tested { get; set; }
        public int? SiteId { get; set; }
        public int? BoxId { get; set; }
        public int MaxRows { get; set; } = 200;
        public bool CustomerView { get; set; } = false;
    }
}

namespace Site_2024.Web.Api.Models
{
    public class PartSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CatagoryId { get; set; }
        public string CatagoryName { get; set; }
        public int MakeId { get; set; }
        public string MakeName { get; set; }
        public int? ModelId { get; set; }
        public string ModelName { get; set; }
        public int? Year { get; set; }
        public string PartNumber { get; set; }
        public bool? Rusted { get; set; }
        public bool? Tested { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Image { get; set; }
        public int? AvailableId { get; set; }
        public string AvailableStatus { get; set; }
        public int? SiteId { get; set; }
        public string SiteName { get; set; }
        public int? BoxId {  get; set; }
        public string BoxName { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}

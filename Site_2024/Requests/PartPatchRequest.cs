namespace Site_2024.Web.Api.Requests
{
    public sealed class PartPatchRequest
    {
        public decimal? Price { get; set; }    
        public int? AvailableId { get; set; }
        public bool? Rusted { get; set; }
        public bool? Tested { get; set; }
        public int? Quantity { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }       
        public int? LocationId { get; set; }      
    }


}

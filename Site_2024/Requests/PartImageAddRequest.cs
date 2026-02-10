namespace Site_2024.Web.Api.Requests
{
    public class PartImageAddRequest
    {
        public int PartId { get; set; }
        public string Url { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

}

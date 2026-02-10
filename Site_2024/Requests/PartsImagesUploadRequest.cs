namespace Site_2024.Web.Api.Requests
{
    public class PartImagesUploadRequest
    {
        public List<IFormFile> Images { get; set; } = new();
        public bool SetFirstAsPrimary { get; set; } = false; // default safe behavior
        public int SortStart { get; set; } = 0; // optional: start sort order at N
    }
}

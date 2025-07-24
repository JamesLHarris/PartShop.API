namespace Site_2024.Web.Api.Models
{
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Shelf Shelf { get; set; }
    }
}

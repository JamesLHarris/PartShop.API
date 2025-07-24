namespace Site_2024.Web.Api.Models
{
    public class Location
    {
        public int Id { get; set; }
        public Site Site {  get; set; }
        public Area Area { get; set; }
        public Aisle Aisle { get; set; }
        public Shelf Shelf { get; set; }
        public Section Section {  get; set; }
        public Box Box { get; set; }

    }
}

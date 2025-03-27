using Microsoft.VisualBasic;
using Site_2024.Web.Api.Models.User;

namespace Site_2024.Web.Api.Models
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Catagory Catagory { get; set; }
        public Make Make { get; set; }
        public int Year { get; set; }
        public string PartNumber {  get; set; }
        public bool Rusted { get; set; }
        public bool Tested { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public Location Location { get; set; }
        public string Image {  get; set; }
        public Available Available { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public Site_2024.Web.Api.Models.User.User User { get; set; }

    }
}

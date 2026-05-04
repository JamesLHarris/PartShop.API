namespace Site_2024.Web.Api.Models
{
    public class PartSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Catagory Catagory { get; set; }
        public Make Make { get; set; }

        public string Year { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public Condition Condition { get; set; }
        public ShippingPolicy ShippingPolicy { get; set; }

        public string Image { get; set; }

        public Available Available { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}

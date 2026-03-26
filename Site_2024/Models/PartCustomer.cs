using System;

namespace Site_2024.Models.Domain.Parts
{
    public class PartCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CatagoryId { get; set; }
        public string CatagoryName { get; set; }
        public int MakeId { get; set; }
        public string MakeName { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string Year { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int ConditionId { get; set; }
        public string ConditionName { get; set; }
        public string Image { get; set; }
        public int AvailableId { get; set; }
        public string AvailableStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}


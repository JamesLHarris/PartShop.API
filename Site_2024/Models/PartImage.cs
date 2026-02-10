using System;

namespace Site_2024.Models.Domain.Parts
{
    public class PartImage
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string Url { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
    }
}

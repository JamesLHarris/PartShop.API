using System;

namespace Site_2024.Models.Domain.RefundRequests
{
    public class RefundRequest
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public decimal Price { get; set; }
        public long? PartShopifyOrderId { get; set; }
        public long? ShopifyOrderId { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByName { get; set; }
        public int? ResolvedByUserId { get; set; }
        public string ResolvedByName { get; set; }
        public DateTime? ResolvedDate { get; set; }
    }
}
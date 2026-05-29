using System;
using System.Collections.Generic;

namespace Site_2024.Models.Domain.RefundRequests
{
    public class RefundRequest
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public string? PartNumber { get; set; }
        public decimal Price { get; set; }
        public long? PartShopifyOrderId { get; set; }
        public long? ShopifyOrderId { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? OrderNumber { get; set; }
        public string? CustomerEmail { get; set; }
        public int? ReturnReasonId { get; set; }
        public string? ReturnReasonName { get; set; }
        public bool RequiresNotes { get; set; }
        public bool RequiresPhotos { get; set; }
        public string? AdminNotes { get; set; }
        public string? DenialReason { get; set; }
        public int ItemCount { get; set; }
        public int PhotoCount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? CreatedByName { get; set; }
        public int? ResolvedByUserId { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public List<RefundRequestItem> Items { get; set; } = new List<RefundRequestItem>();
        public List<RefundRequestPhoto> Photos { get; set; } = new List<RefundRequestPhoto>();
    }

    public class RefundRequestItem
    {
        public int Id { get; set; }
        public int RefundRequestId { get; set; }
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public string? PartNumber { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public long? ShopifyLineItemId { get; set; }
        public int Quantity { get; set; }
        public string? ItemNotes { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class RefundRequestPhoto
    {
        public int Id { get; set; }
        public int RefundRequestId { get; set; }
        public int? RefundRequestItemId { get; set; }
        public string? Url { get; set; }
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
        public int SortOrder { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class ReturnReason
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool RequiresNotes { get; set; }
        public bool RequiresPhotos { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }

    public class ReturnStatus
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsTerminal { get; set; }
        public int SortOrder { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}

using System.Collections.Generic;
using Site_2024.Models.Domain.RefundRequests;
using Site_2024.Models.Requests.RefundRequests;
using Site_2024.Web.Api.Constructors;

namespace Site_2024.Web.Api.Services
{
    public interface IRefundRequestService
    {
        int Add(RefundRequestAddRequest model, int? userId);
        RefundRequest? GetById(int id);
        Paged<RefundRequest>? GetPaginated(int pageIndex, int pageSize, RefundRequestSearchRequest model);
        List<ReturnReason> GetReasons();
        List<ReturnStatus> GetStatuses();
        int AddItem(int refundRequestId, RefundRequestItemAddRequest model);
        int AddPhoto(int refundRequestId, RefundRequestPhotoAddRequest model);
        void UpdateStatus(int id, RefundRequestUpdateStatusRequest model, int userId);
    }
}

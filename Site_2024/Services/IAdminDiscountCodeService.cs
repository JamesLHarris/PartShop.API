using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IAdminDiscountCodeService
    {
        int Add(AdminDiscountCodeAddRequest model, int? userId);
        void Deactivate(int id, AdminDiscountCodeDeactivateRequest model, int? userId);
        AdminDiscountCode? GetById(int id);
        Paged<AdminDiscountCode>? GetPaginated(int pageIndex, int pageSize, AdminDiscountCodeSearchRequest model);
        void MarkShopifyCreated(int id, AdminDiscountCodeShopifyCreatedRequest model);
    }
}
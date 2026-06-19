using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IPartService
    {
        void DeletePart(int id);
        Paged<PartSummary> GetAllPaginated(int pageIndex, int pageSize);
        Paged<PartSummary> GetAvailablePaginated(int pageIndex, int pageSize, int availableId);
        Paged<PartCustomerSummary> GetAvailablePaginatedForCustomer(int pageIndex, int pageSize, int availableId);
        Paged<PartSummary> GetByCategoryPaginated(int pageIndex, int pageSize, int categoryId);
        Paged<PartCustomerSummary> GetByCategoryPaginatedCustomer(int pageIndex, int pageSize, int categoryId);
        Paged<PartSummary> GetByModelPaginated(int pageIndex, int pageSize, int modelId);
        Paged<PartCustomerSummary> GetByModelPaginatedCustomer(int pageIndex, int pageSize, int modelId);
        Part GetPartById(int id);
        Part GetPartByIdCustomer(int id);
        Paged<PartSummary> GetPartsPaginated(int pageIndex, int pageSize);
        int Insert(PartAddRequest model, int userId);
        void PatchPart(int id, PartPatchRequest model, int userId);
        List<PartSearchResult> Search(PartSearchRequest model);
        Paged<PartCustomerSummary> SearchCustomer(int pageIndex, int pageSize, CustomerSearchRequest model);
        void UpdateShopifyIds(int partId, long shopifyProductId, long shopifyVariantId, long shopifyInventoryItemId);
    }
}
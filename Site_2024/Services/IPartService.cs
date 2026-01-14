using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IPartService
    {
        int Insert(PartAddRequest model, int userId);
        void DeletePart(int id);
        Paged<Part> GetAvailablePaginated(int pageIndex, int pageSize, int availableId);
        Paged<Part> GetAvailablePaginatedForCustomer(int pageIndex, int pageSize, int availableId);
        public Paged<Part> GetByModelPaginated(int pageIndex, int pageSize, int modelId);
        public Paged<Part> GetByModelPaginatedCustomer(int pageIndex, int pageSize, int modelId);
        public Paged<Part> GetByCategoryPaginated(int pageIndex, int pageSize, int categoryId);
        public Paged<Part> GetByCategoryPaginatedCustomer(int pageIndex, int pageSize, int categoryId);
        public Part GetPartById(int id);
        public Part GetPartByIdCustomer(int id);
        Paged<Part> GetPartsPaginated(int pageIndex, int pageSize);
        void UpdatePart(PartUpdateRequest model);
        void UpdatePartLocation(PartLocationUpdateRequest model);
        public void PatchPart(int id, PartPatchRequest model /*, int userId */);
        List<PartSearchResult> Search(PartSearchRequest model);
    }
}
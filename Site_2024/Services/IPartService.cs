using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IPartService
    {
        int AddPart(PartAddRequest model, int userId);
        void DeletePart(int id);
        Paged<Part> GetAvailablePaginated(int pageIndex, int pageSize, int availableId);
        Paged<Part> GetAvailablePaginatedForCustomer(int pageIndex, int pageSize, int availableId);
        Part GetPartById(int id);
        Paged<Part> GetPartsPaginated(int pageIndex, int pageSize);
        void UpdatePart(PartUpdateRequest model);
        void UpdatePartLocation(PartLocationUpdateRequest model);
    }
}
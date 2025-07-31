using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface ILocationService
    {
        int AddLocation(LocationAddRequest model);
        void DeleteLocation(int id);
        Location GetLocationById(int id);
        Location GetLocationBySiteId(int id);
        List<Location> GetLocationsAll();
        List<Location> GetHierarchy(int siteId);
        void UpdateLocation(LocationUpdateRequest model);
    }
}
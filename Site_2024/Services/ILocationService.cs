using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface ILocationService
    {
        int AddAisle(AisleAddRequest model);
        int AddArea(AreaAddRequest model);
        int AddBox(BoxAddRequest model);
        int AddLocation(LocationAddRequest model);
        int AddSection(SectionAddRequest model);
        int AddShelf(ShelfAddRequest model);
        int AddSite(SiteAddRequest model);
        void DeleteLocation(int id);
        List<Aisle> GetAisleAll();
        List<Aisle> GetAisleByAreaId(int id);
        List<Area> GetAreaBySiteId(int id);
        List<Area> GetAreasAll();
        List<Box> GetBoxAll();
        List<Box> GetBoxBySectionId(int id);
        Location GetLocationById(int id);
        Location GetLocationBySiteId(int id);
        List<Location> GetLocationsAll();
        List<Section> GetSectionAll();
        List<Section> GetSectionByShelfId(int id);
        List<Shelf> GetShelfAll();
        List<Shelf> GetShelfByAisleId(int id);
        List<Site> GetSitesAll();
        void UpdateLocation(LocationUpdateRequest model);
    }
}
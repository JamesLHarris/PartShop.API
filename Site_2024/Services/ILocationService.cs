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
        int AddSite(SiteAddRequest model);
        int AddSites(SiteAddRequest model);
        void DeleteLocation(int id);
        List<Aisle> GetAisleAll();
        List<Area> GetAreaAll();
        List<Area> GetAreasAll();
        List<Box> GetBoxAll();
        Location GetLocationById(int id);
        List<Location> GetLocationsAll();
        List<Section> GetSectionAll();
        List<Site> GetSiteAll();
        List<Site> GetSitesAll();
        void UpdateLocation(LocationUpdateRequest model);
    }
}
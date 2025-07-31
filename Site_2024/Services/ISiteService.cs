using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface ISiteService
    {
        int AddSite(SiteAddRequest model);
        List<Site> GetSitesAll();
    }
}
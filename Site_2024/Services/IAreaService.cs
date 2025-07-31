using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IAreaService
    {
        int AddArea(AreaAddRequest model);
        List<Area> GetAreaBySiteId(int id);
        List<Area> GetAreasAll();
    }
}
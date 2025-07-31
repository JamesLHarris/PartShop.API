using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IAisleService
    {
        int AddAisle(AisleAddRequest model);
        List<Aisle> GetAisleAll();
        List<Aisle> GetAisleByAreaId(int id);
    }
}
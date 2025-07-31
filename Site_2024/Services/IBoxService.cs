using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IBoxService
    {
        int AddBox(BoxAddRequest model);
        List<Box> GetBoxAll();
        List<Box> GetBoxBySectionId(int id);
    }
}
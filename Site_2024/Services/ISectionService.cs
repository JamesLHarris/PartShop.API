using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface ISectionService
    {
        int AddSection(SectionAddRequest model);
        List<Section> GetSectionAll();
        List<Section> GetSectionByShelfId(int id);
    }
}
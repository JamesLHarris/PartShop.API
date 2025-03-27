using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface ICatagoryService
    {
        int AddCatagory(CatagoryAddRequest model);
        void DeleteCatagory(int id);
        List<Catagory> GetCatagoryAll();
        Catagory GetCatagoryById(int id);
        void UpdateCatagory(CatagoryUpdateRequest model);
    }
}
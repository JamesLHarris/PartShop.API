using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IShelfService
    {
        int AddShelf(ShelfAddRequest model);
        List<Shelf> GetShelfAll();
        List<Shelf> GetShelfByAisleId(int id);
    }
}
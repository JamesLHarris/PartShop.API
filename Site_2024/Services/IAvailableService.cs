using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public interface IAvailableService
    {
        int Add(string status);
        void Delete(int id);
        List<Available> GetAll();
        Available GetById(int id);
    }
}
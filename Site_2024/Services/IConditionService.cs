using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public interface IConditionService
    {
        int Add(string name);
        List<Condition> GetAll();
        Condition GetById(int id);
    }
}
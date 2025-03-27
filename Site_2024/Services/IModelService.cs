using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IModelService
    {
        int AddModel(ModelAddRequest model);
        void DeleteModel(int id);
        Model GetModelById(int id);
        List<Model> GetModelsAll();
        void UpdateModel(ModelUpdateRequest model);
    }
}
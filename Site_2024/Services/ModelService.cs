using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using System.Linq;
using Site_2024.Web.Api.Extensions;
using System.Data.SqlClient;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public class ModelService : IModelService
    {
        private IDataProvider _data;

        public ModelService(IDataProvider data)
        {
            _data = data;
        }

        #region ---GET---

        public Model GetModelById(int id)
        {
            string procName = "[dbo].[Model_GetById]";
            Model? model = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    model = MapSingleModel(reader, ref startingIndex);
                });

            return model;
        }

        public List<Model> GetByMakeId(int Id)
        {
            string procName = "[dbo].[Model_GetByMakeId]";
            List<Model> models = new();

            _data.ExecuteCmd(procName,
                inputParamMapper: (SqlParameterCollection col) =>
                {
                    col.AddWithValue("@makeId", Id);
                },
                singleRecordMapper: (IDataReader reader, short set) =>
                {
                    int startingIndex = 0;
                    Model model = MapSingleModel(reader, ref startingIndex);
                    models.Add(model);
                });

            return models;
        }

        public List<Model> GetModelsAll()
        {
            string procName = "[dbo].[Model_GetAllModels]";

            List<Model> list = new List<Model>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Model model = MapSingleModel(reader, ref i);
                    list.Add(model); // Add the model to the list
                });

            return list;
        }

        #endregion

        #region ---POST&PUT---

        public int AddModel(ModelAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Model_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonModelParams(model, col);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }
            , returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }

        public void UpdateModel(ModelUpdateRequest model)
        {
            string procName = "[dbo].[Model_Update]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonModelParams(model, col);
                col.AddWithValue("@Id", model.Id);
            }
            , returnParameters: null);
        }

        #endregion

        #region ---DELETE---

        public void DeleteModel(int id)
        {
            string procName = "[dbo].[Model_Delete]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            }
            , returnParameters: null);
        }

        #endregion

        #region ---MAPPERS---

        private static void AddCommonModelParams(ModelAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@name", model.Name);
        }

        private Model MapSingleModel(IDataReader reader, ref int startingIndex)
        {
            Model model = new Model();

            model.Id = reader.GetSafeInt32(startingIndex++);
            model.Name = reader.GetSafeString(startingIndex++);

            return model;
        }

        #endregion
    }
}

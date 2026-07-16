using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using Site_2024.Web.Api.Extensions;
using System.Data.SqlClient;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public class ModelService : IModelService
    {
        private readonly IDataProvider _data;

        public ModelService(IDataProvider data)
        {
            _data = data;
        }

        #region ---GET---

        public Model GetModelById(int id)
        {
            const string procName = "[dbo].[Model_GetById]";
            Model? model = null;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    model = MapSingleModel(reader, ref startingIndex);
                });

            return model;
        }

        public List<Model> GetByMakeId(int id)
        {
            const string procName = "[dbo].[Model_GetByMakeId]";
            List<Model> models = new();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@makeId", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    Model model = MapModelForMake(reader, ref startingIndex);
                    models.Add(model);
                });

            return models;
        }

        public List<Model> GetModelsAll()
        {
            const string procName = "[dbo].[Model_GetAllModels]";
            List<Model> list = new();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    Model model = MapSingleModel(reader, ref startingIndex);
                    list.Add(model);
                });

            return list;
        }

        #endregion

        #region ---POST&PUT---

        public int AddModel(ModelAddRequest model)
        {
            int id = 0;
            const string procName = "[dbo].[Model_Insert]";

            _data.ExecuteNonQuery(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonModelParams(model, col);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    col.Add(idOut);
                },
                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;
                    int.TryParse(oId.ToString(), out id);
                });

            return id;
        }

        public void UpdateModel(ModelUpdateRequest model)
        {
            const string procName = "[dbo].[Model_Update]";

            _data.ExecuteNonQuery(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonModelParams(model, col);
                    col.AddWithValue("@Id", model.Id);
                },
                returnParameters: null);
        }

        #endregion

        #region ---DELETE---

        public void DeleteModel(int id)
        {
            const string procName = "[dbo].[Model_Delete]";

            _data.ExecuteNonQuery(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                },
                returnParameters: null);
        }

        #endregion

        #region ---MAPPERS---

        private static void AddCommonModelParams(
            ModelAddRequest model,
            SqlParameterCollection col)
        {
            col.AddWithValue("@name", model.Name);
        }

        // Used by Model_GetById and Model_GetAllModels, which return Id and Name.
        private static Model MapSingleModel(IDataReader reader, ref int startingIndex)
        {
            return new Model
            {
                Id = reader.GetSafeInt32(startingIndex++),
                Name = reader.GetSafeString(startingIndex++)
            };
        }

        // Used only by Model_GetByMakeId, which returns five columns.
        private static Model MapModelForMake(IDataReader reader, ref int startingIndex)
        {
            return new Model
            {
                Id = reader.GetSafeInt32(startingIndex++),
                Name = reader.GetSafeString(startingIndex++),
                MakeId = reader.GetSafeInt32(startingIndex++),
                CompanyMakeId = reader.GetSafeInt32(startingIndex++),
                Company = reader.GetSafeString(startingIndex++)
            };
        }

        #endregion
    }
}

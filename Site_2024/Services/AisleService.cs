using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public class AisleService : IAisleService
    {
        private IDataProvider _data;

        public AisleService(IDataProvider data)
        {
            _data = data;
        }

        public int AddAisle(AisleAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Aisle_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);
                col.AddWithValue("@areaId", model.AreaId);

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
        public List<Aisle> GetAisleAll()
        {
            string procName = "[dbo].[Aisle_GetAll]";

            List<Aisle> list = new List<Aisle>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Aisle aisle = MapSingleAisle(reader, ref i);
                    list.Add(aisle);
                });

            return list;
        }
        public List<Aisle> GetAisleByAreaId(int id)
        {
            string procName = "[dbo].[Aisle_GetByAreaId]";
            List<Aisle> list = new List<Aisle>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@areaId", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Aisle aisle = MapSingleAisle(reader, ref i);
                    list.Add(aisle);
                });

            return list;
        }

        #region ---MAPPER---
        private Aisle MapSingleAisle(IDataReader reader, ref int startingIndex)
        {
            Aisle aisle = new Aisle();

            aisle.Id = reader.GetSafeInt32(startingIndex++);
            aisle.Name = reader.GetSafeString(startingIndex++);

            return aisle;
        }
        #endregion
    }
}


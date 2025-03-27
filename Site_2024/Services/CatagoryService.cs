using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using System.Linq;
using Site_2024.Web.Api.Extensions;
using System.Data.SqlClient;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Requests;
using System.Collections.Generic;

namespace Site_2024.Web.Api.Services
{
    public class CatagoryService : ICatagoryService
    {

        private IDataProvider _data = null;

        public CatagoryService(IDataProvider data)
        {
            _data = data;
        }

        #region ---GET---
        
        public Catagory GetCatagoryById(int id)
        {
            string procName = "[dbo].[Catagory_GetById]";
            Catagory catagory = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    catagory = MapSingleCatagory(reader, ref startingIndex);
                });
            return catagory;
        }

        public List<Catagory> GetCatagoryAll()
        {
            string procName = "[dbo].[Catagory_GetAll]";
            List<Catagory> list = new List<Catagory>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Catagory catagory = MapSingleCatagory(reader, ref i);
                    list.Add(catagory);
                });

            return list;
        }

        #endregion

        #region ---POST&PUT---
        public int AddCatagory(CatagoryAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Catagory_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonCatagoryParams(model, col);

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

        public void UpdateCatagory(CatagoryUpdateRequest model)
        {
            string procName = "[dbo].[Catagory_Update]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonCatagoryParams(model, col);
                col.AddWithValue("@Id", model.Id);
            }
            , returnParameters: null);
        }


        #endregion

        #region ---DELETE---
        public void DeleteCatagory(int id)
        {
            string procName = "[dbo].[Catagory_Delete]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            }
            , returnParameters: null);
        }
        #endregion

        #region ---MAPPER---

        private Catagory MapSingleCatagory(IDataReader reader, ref int startingIndex)
        {
            Catagory catagory = new Catagory();

            catagory.Id = reader.GetSafeInt32(startingIndex++);
            catagory.Name = reader.GetSafeString(startingIndex++);

            return catagory;
        }

        private static void AddCommonCatagoryParams(CatagoryAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@name", model.Name);
        }
        #endregion
    }
}

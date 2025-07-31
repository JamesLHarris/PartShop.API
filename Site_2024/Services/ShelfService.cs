using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.Design;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public class ShelfService : IShelfService
    {
        private IDataProvider _data;

        public ShelfService(IDataProvider data)
        {
            _data = data;
        }

        public List<Shelf> GetShelfAll()
        {
            string procName = "[dbo].[Shelf_GetAll]";

            List<Shelf> list = new List<Shelf>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Shelf shelf = MapSingleShelf(reader, ref i);
                    list.Add(shelf);
                });

            return list;
        }
        public List<Shelf> GetShelfByAisleId(int id)
        {
            string procName = "[dbo].[Shelf_GetByAisleId]";
            List<Shelf> list = new List<Shelf>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@aisleId", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Shelf shelf = MapSingleShelf(reader, ref i);
                    list.Add(shelf);
                });

            return list;
        }

        public int AddShelf(ShelfAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Shelf_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);
                col.AddWithValue("@aisleId", model.AisleId);

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

        #region ---MAPPER---
        private Shelf MapSingleShelf(IDataReader reader, ref int startingIndex)
        {
            Shelf shelf = new Shelf();

            shelf.Id = reader.GetSafeInt32(startingIndex++);
            shelf.Name = reader.GetSafeString(startingIndex++);

            return shelf;
        }
        #endregion
    }
}


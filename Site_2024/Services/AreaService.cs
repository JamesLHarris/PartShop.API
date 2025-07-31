using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public class AreaService : IAreaService
    {
        private IDataProvider _data;

        public AreaService(IDataProvider data)
        {
            _data = data;
        }

        public int AddArea(AreaAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Area_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);
                col.AddWithValue("@siteId", model.SiteId);

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
        public List<Area> GetAreaBySiteId(int id)
        {
            string procName = "[dbo].[Area_GetBySiteId]";
            List<Area> list = new List<Area>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@siteId", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Area area = MapSingleArea(reader, ref i);
                    list.Add(area);
                });

            return list;
        }
        public List<Area> GetAreasAll()
        {
            string procName = "[dbo].[Area_GetAll]";

            List<Area> list = new List<Area>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Area area = MapSingleArea(reader, ref i);
                    list.Add(area);
                });

            return list;
        }

        #region ---MAPPER---
        private Area MapSingleArea(IDataReader reader, ref int startingIndex)
        {
            Area area = new Area();

            area.Id = reader.GetSafeInt32(startingIndex++);
            area.Name = reader.GetSafeString(startingIndex++);

            return area;
        }
        #endregion
    }
}


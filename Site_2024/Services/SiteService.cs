using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public class SiteService : ISiteService
    {
        private readonly IDataProvider _data;

        public SiteService(IDataProvider data)
        {
            _data = data;
        }

        public int AddSite(SiteAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Site_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);

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

        public List<Models.Site> GetSitesAll()
        {
            string procName = "[dbo].[Site_GetAll]";

            List<Models.Site> list = new List<Models.Site>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Models.Site site = MapSingleSite(reader, ref i);
                    list.Add(site);
                });

            return list;
        }

        #region ---MAPPER---
        private Models.Site MapSingleSite(IDataReader reader, ref int startingIndex)
        {
            Models.Site site = new Models.Site();

            site.Id = reader.GetSafeInt32(startingIndex++);
            site.Name = reader.GetSafeString(startingIndex++);

            return site;
        }
        #endregion
    }
}


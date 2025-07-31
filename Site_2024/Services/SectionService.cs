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
    public class SectionService : ISectionService
    {
        private IDataProvider _data;

        public SectionService(IDataProvider data)
        {
            _data = data;
        }

        public List<Section> GetSectionAll()
        {
            string procName = "[dbo].[Section_GetAll]";

            List<Section> list = new List<Section>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Section section = MapSingleSection(reader, ref i);
                    list.Add(section);
                });

            return list;
        }

        public List<Section> GetSectionByShelfId(int id)
        {
            string procName = "[dbo].[Section_GetByShelfId]";
            List<Section> list = new List<Section>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@shelfId", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Section section = MapSingleSection(reader, ref i);
                    list.Add(section);
                });

            return list;
        }

        public int AddSection(SectionAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Section_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);
                col.AddWithValue("@shelfId", model.ShelfId);

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
        private Section MapSingleSection(IDataReader reader, ref int startingIndex)
        {
            Section section = new Section();

            section.Id = reader.GetSafeInt32(startingIndex++);
            section.Name = reader.GetSafeString(startingIndex++);

            return section;
        }
        #endregion
    }
}


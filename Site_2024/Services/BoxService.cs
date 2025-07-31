using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public class BoxService : IBoxService
    {
        private IDataProvider _data;

        public BoxService(IDataProvider data)
        {
            _data = data;
        }

        public List<Box> GetBoxAll()
        {
            string procName = "[dbo].[Box_GetAll]";

            List<Box> list = new List<Box>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Box box = MapSingleBox(reader, ref i);
                    list.Add(box);
                });

            return list;
        }

        public List<Box> GetBoxBySectionId(int id)
        {
            string procName = "[dbo].[Box_GetBySectionId]";
            List<Box> list = new List<Box>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@sectionId", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Box box = MapSingleBox(reader, ref i);
                    list.Add(box);
                });

            return list;
        }
        public int AddBox(BoxAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Box_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);
                col.AddWithValue("@sectionId", model.SectionId);

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
        private Box MapSingleBox(IDataReader reader, ref int startingIndex)
        {
            Box box = new Box();

            box.Id = reader.GetSafeInt32(startingIndex++);
            box.Name = reader.GetSafeString(startingIndex++);

            return box;
        }
        #endregion
    }
}

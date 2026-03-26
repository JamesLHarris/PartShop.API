using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Site_2024.Web.Api.Services
{
    public class ConditionService : IConditionService
    {
        private readonly IDataProvider _data;

        public ConditionService(IDataProvider data)
        {
            _data = data;
        }

        public List<Condition> GetAll()
        {
            string procName = "[dbo].[Conditions_GetAll]";
            List<Condition> list = null;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: null,
                singleRecordMapper: (reader, set) =>
                {
                    int startingIndex = 0;
                    Condition condition = MapSingleCondition(reader, ref startingIndex);

                    list ??= new List<Condition>();
                    list.Add(condition);
                });

            return list;
        }

        public Condition GetById(int id)
        {
            string procName = "[dbo].[Conditions_GetById]";
            Condition condition = null;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: col =>
                {
                    col.AddWithValue("@Id", id);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int startingIndex = 0;
                    condition = MapSingleCondition(reader, ref startingIndex);
                });

            return condition;
        }

        public int Add(string name)
        {
            int id = 0;
            string procName = "[dbo].[Conditions_Insert]";

            _data.ExecuteNonQuery(
                procName,
                inputParamMapper: col =>
                {
                    col.AddWithValue("@Name", name);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    col.Add(idOut);
                },
                returnParameters: col =>
                {
                    id = (int)col["@Id"].Value;
                });

            return id;
        }

        private static Condition MapSingleCondition(IDataReader reader, ref int startingIndex)
        {
            Condition condition = new Condition();

            condition.Id = reader.GetSafeInt32(startingIndex++);
            condition.Name = reader.GetSafeString(startingIndex++);
            condition.DateCreated = reader.GetSafeDateTime(startingIndex++);
            condition.DateModified = reader.GetSafeDateTime(startingIndex++);

            return condition;
        }
    }
}

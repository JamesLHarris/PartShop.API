using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace Site_2024.Web.Api.Services
{
    public class AvailableService : IAvailableService
    {
        private IDataProvider _data;

        public AvailableService(IDataProvider data)
        {
            _data = data;
        }

        public List<Available> GetAll()
        {
            string proc = "Available_GetAll";
            List<Available> list = new();

            _data.ExecuteCmd(proc, null, (reader, set) =>
            {
                int i = 0;
                Available a = new Available
                {
                    Id = reader.GetInt32(i++),
                    Status = reader.GetString(i++)
                };
                list.Add(a);
            });

            return list;
        }

        public Available GetById(int id)
        {
            string proc = "Available_GetById";
            Available item = null;

            _data.ExecuteCmd(proc, (col) =>
            {
                col.AddWithValue("@Id", id);
            }, (reader, set) =>
            {
                int i = 0;
                item = new Available
                {
                    Id = reader.GetInt32(i++),
                    Status = reader.GetString(i++)
                };
            });

            return item;
        }

        public int Add(string status)
        {
            string proc = "Available_Insert";
            int id = 0;

            _data.ExecuteNonQuery(proc, (col) =>
            {
                col.AddWithValue("@status", status);

                SqlParameter outId = new SqlParameter("@Id", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                col.Add(outId);

            }, (col) =>
            {
                id = (int)col["@Id"].Value;
            });

            return id;
        }

        public void Delete(int id)
        {
            string proc = "Available_Delete";

            _data.ExecuteNonQuery(proc, (col) =>
            {
                col.AddWithValue("@Id", id);
            });
        }
    }
}


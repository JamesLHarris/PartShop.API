using System;
using System.Data;
using System.Data.SqlClient;

namespace Site_2024.Web.Api.Services
{
    public class DataProvider : Interfaces.IDataProvider
    {
        private readonly string _connectionString;

        public DataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ExecuteCmd(
            string storedProc,
            Action<SqlParameterCollection> inputParamMapper,
            Action<IDataReader, short> singleRecordMapper,
            Action<SqlParameterCollection> returnParameters = null,
            Action<SqlCommand> cmdModifier = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(storedProc, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    inputParamMapper?.Invoke(cmd.Parameters);
                    cmdModifier?.Invoke(cmd);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        short resultSetIndex = 0;
                        do
                        {
                            while (reader.Read())
                            {
                                singleRecordMapper(reader, resultSetIndex);
                            }
                            resultSetIndex++;
                        } while (reader.NextResult());
                    }
                }
            }

            returnParameters?.Invoke(null); // Assuming null parameters for returnParameters
        }

        public int ExecuteNonQuery(
            string storedProc,
            Action<SqlParameterCollection> inputParamMapper,
            Action<SqlParameterCollection> returnParameters = null)
        {
            int rowsAffected = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(storedProc, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    inputParamMapper?.Invoke(cmd.Parameters);

                    conn.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }
    }
}


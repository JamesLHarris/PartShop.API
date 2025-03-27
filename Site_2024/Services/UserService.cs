using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Interfaces;
using System.Data;
using System.Security.Claims;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Requests;
using Site_2024.Web.Api.Models.User;

namespace Site_2024.Web.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IAuthenticationService<IUserAuthData> _authenticationService;
        private readonly IDataProvider _dataProvider;

        public UserService(IAuthenticationService<IUserAuthData> authenticationService, IDataProvider dataProvider)
        {
            _authenticationService = authenticationService;
            _dataProvider = dataProvider;
        }

        public int Create(UserRegisterRequest model)
        {
            // Hash password (use a real hashing algorithm, this is just for demonstration)
            int userId = 0;
            string password = model.Password;
            string salt = BCrypt.BCryptHelper.GenerateSalt();
            string hashedPassword = BCrypt.BCryptHelper.HashPassword(password, salt);

            // Prepare SQL to create a new user in the database
            string procName = "[dbo].[User_Insert]";

            _dataProvider.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col, salt);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;

                    col.Add(idOut);
                }, returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;

                    int.TryParse(oId.ToString(), out userId);
                });

            return userId;
        }

        public async Task<bool> LogInAsync(string email, string password)
        {
            // Get user details from database
            string procName = "[dbo].[User_GetByEmail]";
            IUserAuthData user = null;

            _dataProvider.ExecuteCmd(procName, inputParamMapper: (paramCollection) =>
            {
                paramCollection.AddWithValue("@Email", email);
            },
            singleRecordMapper: (reader, set) =>
            {
                user = new UserAuthData
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2)
                };
            });

            if (user == null || !BCrypt.BCryptHelper.CheckPassword(password, user.PasswordHash))
            {
                return false; // Login failed
            }

            // Login via authentication service
            await _authenticationService.LogInAsync(user);
            return true; // Login successful
        }

        private static void AddCommonParams(UserRegisterRequest model, SqlParameterCollection col, string salt)
        {
            col.AddWithValue("@Email", model.Email);
            col.AddWithValue("@PasswordHash", BCrypt.BCryptHelper.HashPassword(model.Password, salt));

        }
    }

}
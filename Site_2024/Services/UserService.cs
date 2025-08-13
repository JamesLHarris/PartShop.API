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
using BCrypt.Net;
using Site_2024.Web.Api.Models;


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
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            int role = 4;
            model.RoleId = role;

            // Prepare SQL to create a new user in the database
            string procName = "[dbo].[User_Insert]";

            Console.WriteLine($"HashedPassword: {hashedPassword}");
            Console.WriteLine($"Verifying Password: {password}");
            Console.WriteLine($"Against Hash: {hashedPassword}");
            Console.WriteLine($"Hash Length: {hashedPassword?.Length}");


            _dataProvider.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col, hashedPassword);

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
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3)
                };
            });

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false; // Login failed
            }

            // Login via authentication service
            await _authenticationService.LogInAsync(user);
            return true; // Login successful
        }

        public int GetUserIdByEmail(string email)
        {
            int userId = 0;

            string procName = "[dbo].[User_GetByEmail]";
            _dataProvider.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Email", email);
            },
            singleRecordMapper: delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                userId = reader.GetInt32(startingIndex);
            });

            return userId;
        }

        public User GetUserByEmail(string email)
        {
            string procName = "[dbo].[User_GetByEmailCookie]";
            User? user = null;

            _dataProvider.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Email", email);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    user = MapSingleUser(reader, ref startingIndex);
                });

            return user;
        }

        private static void AddCommonParams(UserRegisterRequest model, SqlParameterCollection col, string hashedPassword)
        {
            col.AddWithValue("@name", model.Name);
            col.AddWithValue("@Email", model.Email);
            col.AddWithValue("@PasswordHash", hashedPassword);
            col.AddWithValue("@RoleId", model.RoleId);

        }

        private User MapSingleUser(IDataReader reader, ref int startingIndex)
        {
            User user = new User();
            user.Role = new Role();

            user.Id = reader.GetSafeInt32(startingIndex++);
            user.Name = reader.GetSafeString(startingIndex++);
            user.Email = reader.GetSafeString(startingIndex++);
            user.DateCreated = reader.GetSafeDateTime(startingIndex++);
            user.Role.Id = reader.GetSafeInt32(startingIndex++);
            user.Role.Name = reader.GetSafeString(startingIndex++);

            return user;
        }
    }

}
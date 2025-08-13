using Site_2024.Web.Api.Models.User;
using Site_2024.Web.Api.Requests;

namespace Site_2024.Web.Api.Services
{
    public interface IUserService
    {
        int Create(UserRegisterRequest model);
        Task<bool> LogInAsync(string email, string password);
        int GetUserIdByEmail(string email);
        public User GetUserByEmail(string email);
    }
}
namespace Site_2024.Web.Api.Interfaces
{
    public interface IAuthenticationService<T>
    {
        Task LogInAsync(T user);
        Task LogOutAsync();
        bool IsLoggedIn();
        T GetCurrentUser();
    }
}
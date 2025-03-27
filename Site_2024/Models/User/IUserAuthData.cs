namespace Site_2024.Web.Api.Models.User
{
    public interface IUserAuthData
    {
        string Email { get; set; }
        int Id { get; set; }
        string PasswordHash { get; set; }
    }
}
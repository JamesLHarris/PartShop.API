namespace Site_2024.Web.Api.Models.User
{
    public interface IUserAuthData
    {
        string Email { get; set; }
        int Id { get; set; }
        string PasswordHash { get; set; }
        int RoleId { get; set; }
        string RoleName { get; set; }
    }
}
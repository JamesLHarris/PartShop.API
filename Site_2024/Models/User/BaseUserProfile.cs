namespace Site_2024.Web.Api.Models.User
{
    public class BaseUserProfile
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Id { get; set; }
        public string AvatarUrl { get; set; }
    }
}

namespace Site_2024.Web.Api.Models.User
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
        public Role Role { get; set; }
    }
}

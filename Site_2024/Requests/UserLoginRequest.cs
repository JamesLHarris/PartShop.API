using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class UserLoginRequest
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [MinLength(3)]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
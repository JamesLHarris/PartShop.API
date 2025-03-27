using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class UserAddRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(10)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least 8 characters with an uppercase letter, lowercase letter, a number, and a symbol.")]
        [MaxLength(100)]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        [MaxLength(100)]
        public string PasswordConfirm { get; set; }

        [Range(1, 3)]
        public int RoleId { get; set; }
    }
}

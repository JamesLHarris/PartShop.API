using Microsoft.AspNetCore.Authentication.Cookies;

namespace Site_2024.Web.Api.Models.User
{
    internal static class AuthenticationDefaults
    {
        public const string AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    }
}

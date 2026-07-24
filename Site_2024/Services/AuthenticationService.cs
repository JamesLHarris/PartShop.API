using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models.User;
using System.Security.Claims;

namespace Site_2024.Web.Api.Services
{
    public class AuthenticationService : IAuthenticationService<IUserAuthData>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogInAsync(IUserAuthData user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            };

            if (!string.IsNullOrWhiteSpace(user.RoleName))
            {
                claims.Add(new Claim(ClaimTypes.Role, CanonicalizeRole(user.RoleName)));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddHours(1),
                AllowRefresh = true
            };

            HttpContext httpContext = _httpContextAccessor.HttpContext!;

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);

            // SignInAsync writes the authentication cookie for the next request,
            // but it does not automatically replace HttpContext.User during the
            // current login request. Set it explicitly so LoginApiController can
            // immediately return the authenticated user profile on the first try.
            httpContext.User = principal;
        }

        public async Task LogOutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public bool IsLoggedIn()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }

        public IUserAuthData GetCurrentUser()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            string idClaim =
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string nameClaim =
                context.User.FindFirst(ClaimTypes.Name)?.Value;
            string emailClaim =
                context.User.FindFirst(ClaimTypes.Email)?.Value;
            string roleClaim =
                context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(idClaim, out int userId))
            {
                return null;
            }

            return new UserAuthData
            {
                Id = userId,
                Name = nameClaim,
                Email = emailClaim,
                RoleName = roleClaim
            };
        }

        private static string CanonicalizeRole(string roleName)
        {
            return roleName switch
            {
                "Admin Low" => "AdminLow",
                "Admin High" => "AdminHigh",
                _ => roleName.Replace(" ", "")
            };
        }
    }
}

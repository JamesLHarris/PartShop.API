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
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, "login");

            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);
        }

        public async Task LogOutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync();
        }

        public bool IsLoggedIn()
        {
            return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public IUserAuthData GetCurrentUser()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value;

            if (!int.TryParse(idClaim, out int userId))
            {
                return null;
            }

            return new UserAuthData
            {
                Id = userId,
                Email = emailClaim
            };
        }

public async Task LogInAsync(IUserAuthData user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email)
    };

        if (roles != null)
        {
            foreach (var r in roles)
            {
                if (!string.IsNullOrWhiteSpace(r))
                    claims.Add(new Claim(ClaimTypes.Role, r));
            }
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties
        {
            IsPersistent = true,
            IssuedUtc = DateTime.UtcNow,
            ExpiresUtc = DateTime.UtcNow.AddHours(1),
            AllowRefresh = true
        };

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }


}
}

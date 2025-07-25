﻿using Microsoft.AspNetCore.Authentication;
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
            if (IsLoggedIn())
            {
                return new UserAuthData
                {
                    Id = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                    Email = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value
                };
            }

            return null;
        }
    }
}

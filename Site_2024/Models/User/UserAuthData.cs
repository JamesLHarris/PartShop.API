﻿namespace Site_2024.Web.Api.Models.User
{
    public class UserAuthData : IUserAuthData
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}

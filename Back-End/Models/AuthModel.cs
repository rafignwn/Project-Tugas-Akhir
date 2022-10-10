using System;
using System.ComponentModel.DataAnnotations;

namespace Web_ASP.Models
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LogoutModel
    {
        public string Username { get; set; }
    }

    public class RefreshTokenModel
    {
        public string Username { get; set; }
        public string RefreshToken { get; set; }
    }
}
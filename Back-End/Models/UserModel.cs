using System;

namespace Web_ASP.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public string RefreshToken { get; set; }
    }
}
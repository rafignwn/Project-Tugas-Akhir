using System;

namespace Web_ASP.Models
{
    public class TokenModel
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
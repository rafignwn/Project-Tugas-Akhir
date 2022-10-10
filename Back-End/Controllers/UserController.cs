using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Web_ASP.Models;

namespace Web_ASP
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private DbUser dbuser = new DbUser();

        [HttpGet("CurrentUser")]
        [Authorize]
        public IActionResult CurrentUser()
        {
            var currentUser = getCurrentUser();
            return new ObjectResult(new
            {
                username = currentUser.Username,
                email = currentUser.EmailAddress,
                role = currentUser.Role
            });
        }

        [AllowAnonymous]
        [HttpGet("CheckToken")]
        public IActionResult CheckToken()
        {
            var refreshToken = Request.Cookies[key: "rftoken"];
            // cek apakah refreshToken ada atau tidak 
            if (refreshToken == null || refreshToken == "")
                return Unauthorized();

            // cek refresh token di database user
            var user = dbuser.GetUserWithRefreshToken(refreshToken);
            // jika refresh token tidak ada di database maka kirim pesan error 403
            if (user.Username == null)
                return Forbid();
            return new ObjectResult(new
            {
                username = user.Username,
                email = user.EmailAddress
            });

            // jika tidak ada kirim error 404 unauthorize
        }

        private UserModel getCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new UserModel
                {
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value,
                    EmailAddress = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value
                };
            }
            return null;
        }
    }
}
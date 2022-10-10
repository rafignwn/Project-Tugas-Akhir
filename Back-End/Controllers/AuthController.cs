using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
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
    public class AuthController : ControllerBase
    {
        // properti config 
        // untuk mengambil beberapa value di config appsetting.json
        private IConfiguration _config;

        // properti untuk mengakses databse
        private DbUser dbUser = new DbUser();

        // constructor AuthController dengan parameter interface config
        public AuthController(IConfiguration config)
        {
            // mengambil config
            _config = config;
        }

        // method untuk login
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginModel userLogin)
        {
            // cek apakah username ada atau tidak
            var user = Authenticate(userLogin);
            // cek username
            if (user != null)
            {
                // jika username ada, cek password yang di masukan benar atau tidak
                if (Cryptography.Decrypt(user.Password).Equals(userLogin.Password))
                {
                    // jika password benar, generate jwt token dan refresh token
                    // genereate jwt token
                    var token = GenerateToken(user);
                    // generate refresh token
                    var refreshToken = GenerateRefreshToken();
                    Response.Cookies.Append(key: "rftoken", value: refreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.Now.AddDays(1),
                        IsEssential = true,
                        SameSite = SameSiteMode.None
                    });
                    // update refresh token di database
                    dbUser.UpdateRefreshToken(user.Username, refreshToken);
                    // kirim response ke klien jwt token dan refresh token
                    return new ObjectResult(new TokenModel
                    {
                        JwtToken = token,
                        RefreshToken = refreshToken
                    });
                }
                // jika passsword salah kirim pesan error
                return Content("{ErrorMessage: 'Password Salah'}");
            }
            // jika username salah kirim pesan error
            return Content("{ErrorMessage: 'Username Tidak Terdaftar'}");
        }
        // end method login

        // method untuk register
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserModel newUser)
        {
            // menambahkan data user ke database 
            if (dbUser.AddUser(newUser) > 0)
            {
                // jika berhasil menambahkan data user kirim pesan berhasil
                return Ok($"Registrasi untuk username '{newUser.Username}' berhasil");
            }
            // jika gagal kirim pesan gagal
            return Ok("Registrasi Gagal");
        }
        // end method registrasi

        // method untuk logout
        [AllowAnonymous]
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            var refreshToken = Request.Cookies[key: "rftoken"];
            // cek apakah refreshToken ada atau tidak 
            if (refreshToken == null || refreshToken == "")
                return NoContent();

            // cek refresh token di database user
            var user = dbUser.GetUserWithRefreshToken(refreshToken);
            // delete rftoken di cookie
            Response.Cookies.Delete(key: "rftoken");
            // cek apakah username ada atau tidak
            if (user.Username != null)
            {
                // jika ada maka hapus refresh token dari database
                if (dbUser.DestroyRefreshToken(user.Username) > 0)
                {
                    // jika berhasil menghapus, kirim response berupa pesan berhasil logout
                    return new ObjectResult(new
                    {
                        message = $"Username {user.Username} Berhasil Logout"
                    });
                }
                // jika gagal menghapus refresh token kirim pesan gagal logout
                return new ObjectResult(new
                {
                    message = $"Username {user.Username} Gagal Logout"
                });
            }
            // jika username / refresh token tidak ada kirim response no content
            return NoContent();
        }

        // method untuk generate refresh token
        public string GenerateRefreshToken()
        {
            // generate random number 
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                // mengirim token
                return Convert.ToBase64String(randomNumber);
            }
        }

        // method untuk authentikasi username
        public UserModel Authenticate(LoginModel userLogin)
        {
            // cek user dengan parameter username
            var currentUser = dbUser.CheckUser(userLogin.Username);
            // jika username ada kirim data user yang sudah diambil
            if (currentUser.Username != null)
            {
                // kirim data user
                return currentUser;
            }
            // kirim null jika data user tidak ada
            return null;
        }
        // end method authenticate

        // Method get access token
        [AllowAnonymous]
        [HttpGet("GetAccessToken")]
        public IActionResult GetAccessToken()
        {
            var refreshToken = Request.Cookies[key: "rftoken"];
            // cek apakah refreshToken ada atau tidak 
            if (refreshToken == null || refreshToken == "")
                return Unauthorized();

            // cek refresh token di database user
            var user = dbUser.GetUserWithRefreshToken(refreshToken);
            // jika refresh token tidak ada di database maka kirim pesan error 403

            if (user.Username != null)
            {
                // jika data user tersedia buat access token baru
                var token = GenerateToken(user);
                // kirim refresh token yang baru
                return new ObjectResult(new
                {
                    accessToken = token
                });
            }
            // jika tidak ada kirim error 403 unauthorize
            return Forbid();
        }
        // end method get access token

        // method untuk mendapatkan access token baru
        [AllowAnonymous]
        [HttpPost("RefreshAccessToken")]
        public IActionResult RefreshAccessToken([FromBody] RefreshTokenModel userToken)
        {
            // cek apakah client mengirim refesh token atau tidak
            if (userToken.RefreshToken == null || userToken.RefreshToken == "")
                // jika client tidak mengirimkan data refresh token maka kirim response no content
                return NoContent();
            // cek refresh token yang dikirim client 
            // dengan refresh token yang ada di databse sama atau tidak
            var refreshToken = dbUser.RefreshTokenCheck(userToken.Username);
            if (refreshToken.Equals(userToken.RefreshToken))
            {
                // jika refresh token sama ambil data user
                var user = dbUser.GetUserWithRefreshToken(userToken.RefreshToken);
                if (user.Username != null)
                {
                    // jika data user tersedia buat access token baru
                    var token = GenerateToken(user);
                    // kirim refresh token yang baru
                    return new ObjectResult(new
                    {
                        newToken = token
                    });
                }
                // jika data user tidak ada kirim  response no content
                return NoContent();
            }
            // jika token tidak sama kirim response 403 forbidden
            return Forbid();
        }
        // end method get new access token

        // method untuk menggenerate access token
        public string GenerateToken(UserModel user)
        {
            // membuat security key dengan key token yang ada di config appsetting.json
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            // membuat signing credentials dengan parameter security key dan algoritma enkripsinya
            // menggunakan algoritma enkripsi Sha256
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // membuat claims baru 
            // data user yang dikalim ada username, email, sama Role
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.Role, user.Role),
            };

            // membuat jwt token dengan parameter issuer, audience, claims, expires token sama signing credentials
            // masa aktif token diset selama 30 detik
            // setelah 30detik access token sudah tidak valid
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddSeconds(30),
                signingCredentials: credentials
            );
            // mengirim access token yang sudah dibuat
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
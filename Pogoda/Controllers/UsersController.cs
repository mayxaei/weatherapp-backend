
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pogoda.Data;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Pogoda.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public UsersController(
            ApplicationDbContext db,
            IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var username = request.Username.Trim();
            var email = request.Email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Wszystkie pola są wymagane.");
            }

            if (username.Length < 3 || username.Length > 50)
            {
                return BadRequest(
                    "Nazwa użytkownika musi mieć od 3 do 50 znaków.");
            }

            if (email.Length > 255 ||
                !new EmailAddressAttribute().IsValid(email))
            {
                return BadRequest("Podaj prawidłowy adres email.");
            }

            if (request.Password.Length < 8)
            {
                return BadRequest("Hasło musi mieć co najmniej 8 znaków.");
            }

            if (request.Password.Length > 128)
            {
                return BadRequest("Hasło może mieć maksymalnie 128 znaków.");
            }

            var emailExists = await _db.Users
                .AnyAsync(user => user.Email == email);

            if (emailExists)
            {
                return Conflict(
                    "Użytkownik z tym adresem email już istnieje.");
            }

            var usernameExists = await _db.Users
                .AnyAsync(user => user.Username == username);

            if (usernameExists)
            {
                return Conflict(
                    "Ta nazwa użytkownika jest już zajęta.");
            }

            var salt = RandomNumberGenerator.GetBytes(16);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                request.Password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash =
                    Convert.ToBase64String(salt) + "." +
                    Convert.ToBase64String(hash),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "Konto z podanym adresem email lub nazwą użytkownika już istnieje.");
            }

            return Ok(new
            {
                message = "Konto zostało utworzone.",
                user.Id,
                user.Username,
                user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(
                    "Email lub nazwa użytkownika i hasło są wymagane.");
            }

            var login = request.Login.Trim();

            var user = await _db.Users.FirstOrDefaultAsync(user =>
                user.Email == login.ToLowerInvariant() ||
                user.Username == login);

            if (user == null)
            {
                return Unauthorized(
                    "Nieprawidłowy email, nazwa użytkownika lub hasło.");
            }

            var parts = user.PasswordHash.Split('.');

            if (parts.Length != 2)
            {
                return Unauthorized(
                    "To konto wymaga ponownej rejestracji.");
            }

            byte[] salt;
            byte[] storedHash;

            try
            {
                salt = Convert.FromBase64String(parts[0]);
                storedHash = Convert.FromBase64String(parts[1]);
            }
            catch (FormatException)
            {
                return Unauthorized(
                    "Nieprawidłowy email, nazwa użytkownika lub hasło.");
            }

            var enteredHash = Rfc2898DeriveBytes.Pbkdf2(
                request.Password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            if (!CryptographicOperations.FixedTimeEquals(
                enteredHash, storedHash))
            {
                return Unauthorized(
                    "Nieprawidłowy email, nazwa użytkownika lub hasło.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            var tokenString =
                new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "Zalogowano pomyślnie.",
                token = tokenString,
                user.Id,
                user.Username,
                user.Email
            });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
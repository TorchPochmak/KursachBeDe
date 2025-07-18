using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Models.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FarmMetricsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        private readonly string BanGuid = GenerateSeededGuid(29052025).ToString();
        public static Guid GenerateSeededGuid(int seed)
        {
            var r = new Random(seed);
            var guid = new byte[16];
            r.NextBytes(guid);
            return new Guid(guid);
        }

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Аутентификация пользователя и выдача JWT токена
        /// </summary>
        /// <param name="login">Данные для входа (email и пароль)</param>
        /// <returns>JWT токен и информация о пользователе</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin login)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    ErrorType = "UserNotFound",
                    Message = "Пользователь с таким email не найден"
                });
            }

            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Unauthorized(new
                {
                    ErrorType = "InvalidPassword",
                    Message = "Неверный пароль"
                });
            }

            if (user.Name?.StartsWith($"[{BanGuid}]") == true)
            {
                return Unauthorized(new
                {
                    ErrorType = "UserBanned",
                    Message = "Ваш аккаунт заблокирован. Обратитесь к администратору."
                });
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"])),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new AuthResponse
            {
                Token = tokenString,
                UserName = user.Name ?? string.Empty,
                Role = user.Role?.Name ?? "User",
                UserId = user.Id
            });
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="register">Данные для регистрации нового пользователя</param>
        /// <returns>Сообщение об успешной регистрации</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister register)
        {
            if (await _context.Users.AnyAsync(u => u.Email == register.Email))
            {
                return BadRequest("User with this email already exists");
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                return BadRequest("User role not found");
            }

            var newUser = new User
            {
                Name = register.Name,
                Email = register.Email,
                Phone = register.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                RoleId = userRole.Id
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully" });
        }

        /// <summary>
        /// Получение информации о пользователе по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Детальная информация о пользователе</returns>
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Settlement)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Phone,
                Role = user.Role?.Name,
                SettlementId = user.Settlement?.Id,
                Settlement = user.Settlement?.Name
            });
        }

        /// <summary>
        /// Обновление данных пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="request">Новые данные пользователя</param>
        /// <returns>Сообщение об успешном обновлении</returns>
        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Name = request.Name ?? user.Name;
            user.Email = request.Email ?? user.Email;
            user.Phone = request.Phone ?? user.Phone;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "User updated successfully" });
        }

        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Сообщение об успешном удалении</returns>
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User deleted successfully" });
        }

        public class UserUpdateRequest
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Password { get; set; }
        }
    }
}
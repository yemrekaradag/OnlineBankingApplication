using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineBankingApplication.Context;
using OnlineBankingApplication.Entities;
using OnlineBankingApplication.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineBankingApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BaseDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(BaseDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
            {
                throw new BadRequestException("Username already exists.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(User user)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Username == user.Username);
            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(user.PasswordHash, existingUser.PasswordHash))
            {
                throw new BadRequestException("Invalid username or password.");
            }

            var token = GenerateJwtToken(existingUser);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

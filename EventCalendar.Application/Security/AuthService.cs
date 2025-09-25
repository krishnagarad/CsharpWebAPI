using EventCalendar.Application.Contracts.Security;
using EventCalendar.Application.Contracts.Security.Models;
using EventCalendar.Data;
using EventCalendar.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EventCalendar.Application.Security
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthService(UserDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }
        public async Task<TokenResponseDto> LoginAsync(UserDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
            if (user == null)
            {
                return null;
            }
            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, userDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }
            var response = new TokenResponseDto { AccessToken = CreateToken(user), RefreshToken = await GenerateAndSaveRefreshTokenAsync(user) };
            return response;
        }

        public async Task<User?> RegisterAsync(UserDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return null;
            }

            var user = new User();
            var hasedPassord = new PasswordHasher<User>().HashPassword(user, userDto.Password);
            user.Username = userDto.Username;
            user.PasswordHash = hasedPassord;
            user.Role = userDto.Role;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return null;
            }
            return user;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return refreshToken;
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetValue<string>("AppSettings:Token")!
                ));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return null;
            }

            var response = new TokenResponseDto
            {
                AccessToken = CreateToken(user.Result!),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Result!)
            };

            return response;
        }
    }
}

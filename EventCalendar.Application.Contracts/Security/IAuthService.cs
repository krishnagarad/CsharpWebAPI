using EventCalendar.Application.Contracts.Security.Models;
using EventCalendar.Domain.Entities;

namespace EventCalendar.Application.Contracts.Security
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto userDto);
        Task<TokenResponseDto> LoginAsync(UserDto userDto);
        Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}

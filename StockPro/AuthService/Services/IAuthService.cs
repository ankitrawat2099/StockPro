using AuthService.DTOs;

namespace AuthService.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequest request);
    Task<string> LoginAsync(LoginRequest request);
    string Logout(string token);
    bool ValidateToken(string token);
    string RefreshToken(string token);
    Task<UserResponseDto> GetUserByIdAsync(Guid userId);
    Task<UserResponseDto> GetUserByEmailAsync(string email);
    Task<string> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<string> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<string> DeactivateUserAsync(Guid userId);
    Task<List<UserResponseDto>> GetAllUsersAsync();
}
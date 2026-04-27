using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Repositories;
using AuthService.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly IAuthRepository _repository;
    private readonly AuthDbContext _context;
    private readonly IConfiguration _config;

    public AuthServiceImpl(IAuthRepository repository, AuthDbContext context, IConfiguration config)
    {
        _repository = repository;
        _context = context;
        _config = config;
    }

    //Mapping
    private UserResponseDto MapToDto(AppUser user)
    {
        return new UserResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            Department = user.Department,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    //Register
    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var exists = await _repository.ExistsByEmailAsync(request.Email);

        if (exists)
            throw new Exception("User already exists");

        var user = new AppUser
        {
            UserId = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = request.Role?.ToUpper() ?? "STAFF",
            Department = request.Department,
            IsActive = true,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return "User registered successfully";
    }

    private static Claim[] BuildTokenClaims(AppUser user)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            // Include both claim styles so every downstream service can authorize consistently.
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("role", user.Role)
        ];
    }

    private static string GetClaimValue(JwtSecurityToken token, params string[] types)
    {
        foreach (var type in types)
        {
            var value = token.Claims.FirstOrDefault(claim => claim.Type == type)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        throw new UnauthorizedAccessException("Required token claim is missing");
    }

    //Login
    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await _repository.FindByEmailAsync(request.Email);

        if (user == null)
            throw new Exception("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is deactivated");

        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValid)
            throw new Exception("Invalid email or password");

        user.LastLoginAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var claims = BuildTokenClaims(user);

        var keyString = _config["Jwt:Key"] ?? throw new Exception("JWT Key missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //Logout
    public string Logout(string token)
    {
        return "User logged out successfully";
    }

    // Validate Token
    public bool ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Refresh Token
    public string RefreshToken(string token)
    {
        if (!ValidateToken(token))
            throw new UnauthorizedAccessException("Invalid token");

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var userId = GetClaimValue(jwtToken, ClaimTypes.NameIdentifier);
        var email = GetClaimValue(jwtToken, ClaimTypes.Email);
        var role = GetClaimValue(jwtToken, ClaimTypes.Role, "role");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var newToken = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(newToken);
    }

    // Get User By Id
    public async Task<UserResponseDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _repository.FindByUserIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        return MapToDto(user);
    }

    //Get User By Email
    public async Task<UserResponseDto> GetUserByEmailAsync(string email)
    {
        var user = await _repository.FindByEmailAsync(email);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        return MapToDto(user);
    }

    //Update Profile
    public async Task<string> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _repository.FindByUserIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.Department = request.Department;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return "Profile updated successfully";
    }

    //Change Password
    public async Task<string> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _repository.FindByUserIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        var isValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);

        if (!isValid)
            throw new UnauthorizedAccessException("Old password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return "Password changed successfully";
    }

    // Deactivate
    public async Task<string> DeactivateUserAsync(Guid userId)
    {
        var user = await _repository.FindByUserIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.IsActive = false;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return "User deactivated successfully";
    }

    // Get All Users
    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapToDto).ToList();
    }
}

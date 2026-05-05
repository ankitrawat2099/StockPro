using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    //Register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _service.RegisterAsync(request);

        return Ok(new RegisterResponseDto
        {
            Message = result
        });
    }

    //Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _service.LoginAsync(request);

        return Ok(new LoginResponseDto
        {
            Token = token
        });
    }

    //Get Profile (JWT based)
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var user = await _service.GetUserByIdAsync(Guid.Parse(userId));
        return Ok(user);
    }

    //Update Profile (JWT based)
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var result = await _service.UpdateProfileAsync(Guid.Parse(userId), request);

        return Ok(new { message = result });
    }

    //Change Password
    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var result = await _service.ChangePasswordAsync(Guid.Parse(userId), request);

        return Ok(new { message = result });
    }

    //Get All Users (Admin only)
    [Authorize(Roles = "ADMIN")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _service.GetAllUsersAsync();
        return Ok(users);
    }

    //Deactivate User (Admin only)
    [Authorize(Roles = "ADMIN")]
    [HttpPut("deactivate")]
    public async Task<IActionResult> DeactivateUser([FromQuery] Guid userId)
    {
        var result = await _service.DeactivateUserAsync(userId);
        return Ok(new { message = result });
    }

    //Refresh Token
    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] string token)
    {
        var newToken = _service.RefreshToken(token);
        return Ok(new { token = newToken });
    }

    //Logout
    [HttpPost("logout")]
    public IActionResult Logout([FromBody] string token)
    {
        var result = _service.Logout(token);
        return Ok(new { message = result });
    }
}
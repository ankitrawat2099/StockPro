using System;

namespace AuthService.DTOs;

public class UserResponseDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public string Department { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
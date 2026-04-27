using System.ComponentModel.DataAnnotations;
namespace AuthService.Entities;

public class AppUser
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Phone]
    public string Phone { get; set; }

    [Required]
    public string Role { get; set; }

    public string Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

    public DateTime? LastLoginAt { get; set; }
}

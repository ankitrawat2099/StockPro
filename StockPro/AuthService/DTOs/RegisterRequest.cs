using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class RegisterRequest
{
    [Required]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    public string Phone { get; set; }

    [Required]
    public string Role { get; set; }

    public string Department { get; set; }
}
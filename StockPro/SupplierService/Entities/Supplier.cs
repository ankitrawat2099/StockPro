using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(100)]
    public string ContactPerson { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Phone]
    public string Phone { get; set; }

    [MaxLength(250)]
    public string Address { get; set; }

    [MaxLength(100)]
    public string City { get; set; }

    [MaxLength(100)]
    public string Country { get; set; }

    [MaxLength(50)]
    public string TaxId { get; set; }

    [Required]
    public string PaymentTerms { get; set; }

    [Range(0, 365)]
    public int LeadTimeDays { get; set; }

    [Range(0, 5)]
    public double Rating { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}
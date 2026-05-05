using System.ComponentModel.DataAnnotations;
public class CreateSupplierDto
{
    [Required]
    public string Name { get; set; }
    public string ContactPerson { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Phone { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    public string TaxId { get; set; }

    public string PaymentTerms { get; set; }

    public int LeadTimeDays { get; set; }
}
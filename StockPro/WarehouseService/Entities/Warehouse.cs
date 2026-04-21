using System.ComponentModel.DataAnnotations;
public class Warehouse
{
    [Key]
    public int WarehouseId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Location { get; set; }

    public string Address { get; set; }

    public int ManagerId { get; set; }

    public int Capacity { get; set; }

    public int UsedCapacity { get; set; }

    public bool IsActive { get; set; } = true;

    public string Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;

public class WarehouseDto
{
    [Required]
    public string Name { get; set; }

    public string Location { get; set; }
    public string Address { get; set; }

    public int ManagerId { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    public string Phone { get; set; }
}
namespace WarehouseApi.Domain.Entities;

public class Supplier
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string EdrpouCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

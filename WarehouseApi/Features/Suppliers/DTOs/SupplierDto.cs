namespace WarehouseApi.Features.Suppliers.DTOs;

public class SupplierDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string EdrpouCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

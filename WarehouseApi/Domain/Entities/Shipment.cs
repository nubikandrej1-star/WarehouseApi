namespace WarehouseApi.Domain.Entities;

public class Shipment
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Created";
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
}

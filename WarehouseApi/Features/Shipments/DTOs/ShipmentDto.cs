namespace WarehouseApi.Features.Shipments.DTOs;

public sealed record ShipmentDto(
    int Id,
    string TrackingNumber,
    string RecipientName,
    string DestinationAddress,
    string Status,
    int ProductId,
    string? ProductName,
    int Quantity,
    DateTime CreatedAt,
    DateTime? DeliveredAt);

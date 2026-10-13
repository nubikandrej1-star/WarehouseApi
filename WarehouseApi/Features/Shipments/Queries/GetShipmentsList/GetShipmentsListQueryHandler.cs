using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Queries.GetShipmentsList;

public sealed class GetShipmentsListQueryHandler(WarehouseDbContext context)
    : IRequestHandler<GetShipmentsListQuery, List<ShipmentDto>>
{
    public async Task<List<ShipmentDto>> Handle(GetShipmentsListQuery query, CancellationToken cancellationToken)
    {
        var shipments = context.Shipments.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim();
            shipments = shipments.Where(shipment => shipment.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Recipient))
        {
            var recipient = query.Recipient.Trim();
            shipments = shipments.Where(shipment => shipment.RecipientName.Contains(recipient));
        }

        return await shipments
            .OrderByDescending(shipment => shipment.CreatedAt)
            .Select(shipment => new ShipmentDto(
                shipment.Id,
                shipment.TrackingNumber,
                shipment.RecipientName,
                shipment.DestinationAddress,
                shipment.Status,
                shipment.ProductId,
                shipment.Product == null ? null : shipment.Product.Name,
                shipment.Quantity,
                shipment.CreatedAt,
                shipment.DeliveredAt))
            .ToListAsync(cancellationToken);
    }
}

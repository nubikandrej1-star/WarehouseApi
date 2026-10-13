using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.Exceptions;
using WarehouseApi.Data;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Queries.GetShipmentById;

public sealed class GetShipmentByIdQueryHandler(WarehouseDbContext context)
    : IRequestHandler<GetShipmentByIdQuery, ShipmentDto>
{
    public async Task<ShipmentDto> Handle(GetShipmentByIdQuery query, CancellationToken cancellationToken)
    {
        return await context.Shipments
            .AsNoTracking()
            .Where(shipment => shipment.Id == query.Id)
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
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Відвантаження з ID {query.Id} не знайдено.");
    }
}

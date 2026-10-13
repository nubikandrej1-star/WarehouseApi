using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.Exceptions;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Commands.CreateShipment;

public sealed class CreateShipmentCommandHandler(WarehouseDbContext context)
    : IRequestHandler<CreateShipmentCommand, ShipmentDto>
{
    public async Task<ShipmentDto> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .SingleOrDefaultAsync(product => product.Id == command.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Товар з ID {command.ProductId} не знайдено.");

        if (product.StockQuantity < command.Quantity)
        {
            throw new InvalidOperationException(
                $"Недостатньо товару '{product.Name}' на складі. Доступно: {product.StockQuantity}, потрібно: {command.Quantity}.");
        }

        var now = DateTime.UtcNow;
        var trackingSuffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var trackingNumber = $"TRK-{now:yyyy}-{trackingSuffix}";

        product.StockQuantity -= command.Quantity;
        product.UpdatedAt = now;

        var shipment = new Shipment
        {
            TrackingNumber = trackingNumber,
            RecipientName = command.RecipientName.Trim(),
            DestinationAddress = command.DestinationAddress.Trim(),
            Status = "Created",
            ProductId = product.Id,
            Quantity = command.Quantity,
            CreatedAt = now
        };

        context.Shipments.Add(shipment);
        context.StockTransactions.Add(new StockTransaction
        {
            ProductId = product.Id,
            QuantityChange = -command.Quantity,
            Reason = $"Відвантаження {trackingNumber} отримувачу '{shipment.RecipientName}'",
            Timestamp = now
        });

        // One SaveChangesAsync keeps stock, shipment and stock history in the same EF transaction.
        await context.SaveChangesAsync(cancellationToken);

        return new ShipmentDto(
            shipment.Id,
            shipment.TrackingNumber,
            shipment.RecipientName,
            shipment.DestinationAddress,
            shipment.Status,
            shipment.ProductId,
            product.Name,
            shipment.Quantity,
            shipment.CreatedAt,
            shipment.DeliveredAt);
    }
}

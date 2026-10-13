using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.Exceptions;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Commands.UpdateShipmentStatus;

public sealed class UpdateShipmentStatusCommandHandler(WarehouseDbContext context)
    : IRequestHandler<UpdateShipmentStatusCommand, ShipmentDto>
{
    public async Task<ShipmentDto> Handle(UpdateShipmentStatusCommand command, CancellationToken cancellationToken)
    {
        var shipment = await context.Shipments
            .SingleOrDefaultAsync(shipment => shipment.Id == command.ShipmentId, cancellationToken)
            ?? throw new NotFoundException($"Відвантаження з ID {command.ShipmentId} не знайдено.");

        var newStatus = command.NewStatus.Trim();
        if (string.Equals(shipment.Status, "Delivered", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(shipment.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Неможливо змінити статус відвантаження '{shipment.Status}', оскільки це кінцевий статус.");
        }

        if (string.Equals(shipment.Status, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Відвантаження вже має статус '{shipment.Status}'.");
        }

        // The validator restricts NewStatus to the three supported transitions.
        if (!string.Equals(shipment.Status, "Created", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(shipment.Status, "InTransit", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Невідомий поточний статус відвантаження '{shipment.Status}'.");
        }

        if (string.Equals(newStatus, "InTransit", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(shipment.Status, "Created", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Перевести відвантаження в InTransit можна лише зі статусу Created.");
        }

        var now = DateTime.UtcNow;
        string canonicalStatus = newStatus.Equals("InTransit", StringComparison.OrdinalIgnoreCase)
            ? "InTransit"
            : newStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase)
                ? "Delivered"
                : "Cancelled";

        if (canonicalStatus == "Delivered")
        {
            shipment.DeliveredAt = now;
        }
        else if (canonicalStatus == "Cancelled")
        {
            var product = await context.Products
                .SingleOrDefaultAsync(product => product.Id == shipment.ProductId, cancellationToken)
                ?? throw new NotFoundException($"Товар з ID {shipment.ProductId}, пов'язаний із відвантаженням, не знайдено.");

            product.StockQuantity = checked(product.StockQuantity + shipment.Quantity);
            product.UpdatedAt = now;

            context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                QuantityChange = shipment.Quantity,
                Reason = $"Повернення товару на склад після скасування відвантаження {shipment.TrackingNumber}",
                Timestamp = now
            });
        }

        shipment.Status = canonicalStatus;
        await context.SaveChangesAsync(cancellationToken);

        var productName = await context.Products.AsNoTracking()
            .Where(product => product.Id == shipment.ProductId)
            .Select(product => product.Name)
            .SingleOrDefaultAsync(cancellationToken);

        return new ShipmentDto(
            shipment.Id,
            shipment.TrackingNumber,
            shipment.RecipientName,
            shipment.DestinationAddress,
            shipment.Status,
            shipment.ProductId,
            productName,
            shipment.Quantity,
            shipment.CreatedAt,
            shipment.DeliveredAt);
    }
}

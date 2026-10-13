using MediatR;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Commands.CreateShipment;

public sealed record CreateShipmentCommand(
    string RecipientName,
    string DestinationAddress,
    int ProductId,
    int Quantity) : IRequest<ShipmentDto>;

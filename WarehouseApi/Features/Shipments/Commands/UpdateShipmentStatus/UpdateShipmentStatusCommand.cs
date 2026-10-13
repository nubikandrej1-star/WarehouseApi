using MediatR;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Commands.UpdateShipmentStatus;

public sealed record UpdateShipmentStatusCommand(int ShipmentId, string NewStatus) : IRequest<ShipmentDto>;

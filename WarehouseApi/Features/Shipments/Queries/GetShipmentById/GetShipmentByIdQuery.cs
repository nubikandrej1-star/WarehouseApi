using MediatR;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Queries.GetShipmentById;

public sealed record GetShipmentByIdQuery(int Id) : IRequest<ShipmentDto>;

using MediatR;
using WarehouseApi.Features.Shipments.DTOs;

namespace WarehouseApi.Features.Shipments.Queries.GetShipmentsList;

public sealed record GetShipmentsListQuery(string? Status, string? Recipient) : IRequest<List<ShipmentDto>>;

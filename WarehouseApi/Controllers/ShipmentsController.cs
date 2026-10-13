using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseApi.Features.Shipments.Commands.CreateShipment;
using WarehouseApi.Features.Shipments.Commands.UpdateShipmentStatus;
using WarehouseApi.Features.Shipments.DTOs;
using WarehouseApi.Features.Shipments.Queries.GetShipmentById;
using WarehouseApi.Features.Shipments.Queries.GetShipmentsList;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/shipments")]
public sealed class ShipmentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ShipmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShipmentDto>>> GetList(
        [FromQuery] string? status,
        [FromQuery] string? recipient,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShipmentsListQuery(status, recipient), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShipmentByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentDto>> Create(
        [FromBody] CreateShipmentCommand command,
        CancellationToken cancellationToken)
    {
        var created = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentDto>> UpdateStatus(
        int id,
        [FromBody] UpdateShipmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(new UpdateShipmentStatusCommand(id, request.NewStatus), cancellationToken);
        return Ok(updated);
    }

    public sealed record UpdateShipmentStatusRequest(string NewStatus);
}

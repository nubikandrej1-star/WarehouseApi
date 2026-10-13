using Microsoft.AspNetCore.Mvc;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Products.DTOs;
using WarehouseApi.Features.Suppliers.Commands;
using WarehouseApi.Features.Suppliers.DTOs;
using WarehouseApi.Features.Suppliers.Queries;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly IQueryHandler<GetSuppliersListQuery, List<SupplierDto>> _listHandler;
    private readonly IQueryHandler<GetSupplierByIdQuery, SupplierDto?> _getByIdHandler;
    private readonly ICommandHandler<CreateSupplierCommand, SupplierDto> _createHandler;
    private readonly ICommandHandler<RestockProductFromSupplierCommand, ProductDto> _restockHandler;

    public SuppliersController(
        IQueryHandler<GetSuppliersListQuery, List<SupplierDto>> listHandler,
        IQueryHandler<GetSupplierByIdQuery, SupplierDto?> getByIdHandler,
        ICommandHandler<CreateSupplierCommand, SupplierDto> createHandler,
        ICommandHandler<RestockProductFromSupplierCommand, ProductDto> restockHandler)
    {
        _listHandler = listHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _restockHandler = restockHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SupplierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? onlyActive = true,
        CancellationToken token = default)
    {
        var suppliers = await _listHandler.HandleAsync(
            new GetSuppliersListQuery(searchTerm, onlyActive), token);
        return Ok(suppliers);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken token)
    {
        var supplier = await _getByIdHandler.HandleAsync(new GetSupplierByIdQuery(id), token);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierCommand command,
        CancellationToken token)
    {
        try
        {
            var created = await _createHandler.HandleAsync(command, token);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DuplicateSupplierCodeException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("restock")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restock(
        [FromBody] RestockProductFromSupplierCommand command,
        CancellationToken token)
    {
        try
        {
            var updated = await _restockHandler.HandleAsync(command, token);
            return Ok(updated);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}

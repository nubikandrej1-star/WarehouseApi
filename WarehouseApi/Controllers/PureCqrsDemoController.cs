using Microsoft.AspNetCore.Mvc;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Products.Commands;
using WarehouseApi.Features.Products.DTOs;
using WarehouseApi.Features.Products.Queries;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/pure-cqrs-demo")]
public class PureCqrsDemoController : ControllerBase
{
    private readonly IQueryHandler<PureGetProductsListQuery, List<ProductDto>> _listHandler;
    private readonly IQueryHandler<PureGetProductByIdQuery, ProductDto?> _getByIdHandler;
    private readonly ICommandHandler<PureCreateProductCommand, ProductDto> _createHandler;
    private readonly ICommandHandler<PureUpdateStockCommand, ProductDto?> _updateStockHandler;
    private readonly ICommandHandler<PureDeleteProductCommand, bool> _deleteHandler;

    public PureCqrsDemoController(
        IQueryHandler<PureGetProductsListQuery, List<ProductDto>> listHandler,
        IQueryHandler<PureGetProductByIdQuery, ProductDto?> getByIdHandler,
        ICommandHandler<PureCreateProductCommand, ProductDto> createHandler,
        ICommandHandler<PureUpdateStockCommand, ProductDto?> updateStockHandler,
        ICommandHandler<PureDeleteProductCommand, bool> deleteHandler)
    {
        _listHandler = listHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateStockHandler = updateStockHandler;
        _deleteHandler = deleteHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? searchTerm,
        [FromQuery] string? category,
        [FromQuery] bool? onlyInStock,
        CancellationToken token)
    {
        var products = await _listHandler.HandleAsync(
            new PureGetProductsListQuery(searchTerm, category, onlyInStock), token);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken token)
    {
        var product = await _getByIdHandler.HandleAsync(new PureGetProductByIdQuery(id), token);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] PureCreateProductCommand command,
        CancellationToken token)
    {
        var created = await _createHandler.HandleAsync(command, token);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:int}/stock")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(
        int id,
        [FromBody] UpdateStockRequest request,
        CancellationToken token)
    {
        try
        {
            var updated = await _updateStockHandler.HandleAsync(
                new PureUpdateStockCommand(id, request.QuantityChange, request.Reason), token);

            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        var deleted = await _deleteHandler.HandleAsync(new PureDeleteProductCommand(id), token);
        return deleted ? NoContent() : NotFound();
    }

    public sealed record UpdateStockRequest(int QuantityChange, string Reason);
}

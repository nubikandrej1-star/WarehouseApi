using Microsoft.AspNetCore.Mvc;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Products.Commands;
using WarehouseApi.Features.Products.DTOs;
using WarehouseApi.Features.Products.Queries;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ICommandHandler<PureCreateProductCommand, ProductDto> _commandHandler;
    private readonly IQueryHandler<PureGetProductByIdQuery, ProductDto?> _queryHandler;

    public ProductsController(ICommandHandler<PureCreateProductCommand, ProductDto> commandHandler, IQueryHandler<PureGetProductByIdQuery, ProductDto?> queryHandler)
    {
        _commandHandler = commandHandler;
        _queryHandler = queryHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PureCreateProductCommand command)
    {
        var created = await _commandHandler.HandleAsync(command);
        return Ok(created);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _queryHandler.HandleAsync(new PureGetProductByIdQuery(id));
        return product is { } ? Ok(product) : NotFound();
    }
}
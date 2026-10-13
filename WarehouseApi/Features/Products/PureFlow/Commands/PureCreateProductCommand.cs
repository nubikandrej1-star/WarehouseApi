using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Domain.Entities;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Commands
{
    public record PureCreateProductCommand(string Name,
        string Sku, string Description,
        decimal Price, int Stock,
        string Category) : ICommand<ProductDto>;
}

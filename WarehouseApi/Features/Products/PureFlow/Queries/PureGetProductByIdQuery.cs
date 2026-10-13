using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Queries
{
    public record PureGetProductByIdQuery(int Id) : IQuery<ProductDto?>;
}

using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Queries;

public record PureGetProductsListQuery(
    string? SearchTerm = null,
    string? Category = null,
    bool? OnlyInStock = null) : IQuery<List<ProductDto>>;

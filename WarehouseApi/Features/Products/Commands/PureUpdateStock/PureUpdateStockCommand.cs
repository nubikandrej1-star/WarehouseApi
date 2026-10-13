using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Commands;

public record PureUpdateStockCommand(int ProductId, int QuantityChange, string Reason)
    : ICommand<ProductDto?>;

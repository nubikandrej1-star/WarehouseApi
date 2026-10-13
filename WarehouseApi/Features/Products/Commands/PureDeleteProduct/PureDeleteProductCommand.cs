using WarehouseApi.Common.PureCqrs;

namespace WarehouseApi.Features.Products.Commands;

public record PureDeleteProductCommand(int Id) : ICommand<bool>;

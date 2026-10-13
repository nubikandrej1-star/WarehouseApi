using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Suppliers.DTOs;

namespace WarehouseApi.Features.Suppliers.Queries;

public record GetSupplierByIdQuery(int Id) : IQuery<SupplierDto?>;

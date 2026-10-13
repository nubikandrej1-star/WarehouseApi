using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Features.Suppliers.DTOs;

namespace WarehouseApi.Features.Suppliers.Queries;

public record GetSuppliersListQuery(string? SearchTerm = null, bool? OnlyActive = true)
    : IQuery<List<SupplierDto>>;

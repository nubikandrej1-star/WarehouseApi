using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Features.Suppliers.DTOs;

namespace WarehouseApi.Features.Suppliers.Queries;

public class GetSuppliersListQueryHandler
    : IQueryHandler<GetSuppliersListQuery, List<SupplierDto>>
{
    private readonly WarehouseDbContext _context;

    public GetSuppliersListQueryHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> HandleAsync(
        GetSuppliersListQuery query,
        CancellationToken token = default)
    {
        var suppliers = _context.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim();
            suppliers = suppliers.Where(s =>
                s.CompanyName.Contains(searchTerm) || s.EdrpouCode.Contains(searchTerm));
        }

        if (query.OnlyActive == true)
        {
            suppliers = suppliers.Where(s => s.IsActive);
        }

        return await suppliers
            .OrderBy(s => s.CompanyName)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                CompanyName = s.CompanyName,
                EdrpouCode = s.EdrpouCode,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(token);
    }
}

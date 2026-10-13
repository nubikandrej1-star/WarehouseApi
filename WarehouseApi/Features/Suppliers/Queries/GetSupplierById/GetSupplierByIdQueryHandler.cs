using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Features.Suppliers.DTOs;

namespace WarehouseApi.Features.Suppliers.Queries;

public class GetSupplierByIdQueryHandler
    : IQueryHandler<GetSupplierByIdQuery, SupplierDto?>
{
    private readonly WarehouseDbContext _context;

    public GetSupplierByIdQueryHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto?> HandleAsync(
        GetSupplierByIdQuery query,
        CancellationToken token = default)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == query.Id)
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
            .FirstOrDefaultAsync(token);
    }
}

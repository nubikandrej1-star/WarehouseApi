using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Queries;

public class PureGetProductsListQueryHandler
    : IQueryHandler<PureGetProductsListQuery, List<ProductDto>>
{
    private readonly WarehouseDbContext _context;

    public PureGetProductsListQueryHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> HandleAsync(
        PureGetProductsListQuery query,
        CancellationToken token = default)
    {
        var products = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim();
            products = products.Where(p =>
                p.Name.Contains(searchTerm) || p.Sku.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim();
            products = products.Where(p => p.Category == category);
        }

        if (query.OnlyInStock == true)
        {
            products = products.Where(p => p.StockQuantity > 0);
        }

        return await products
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(token);
    }
}

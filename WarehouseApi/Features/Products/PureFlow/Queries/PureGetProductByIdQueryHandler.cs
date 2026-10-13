using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Queries
{
    public class PureGetProductByIdQueryHandler : IQueryHandler<PureGetProductByIdQuery, ProductDto?>
    {
        private readonly WarehouseDbContext _context;

        public PureGetProductByIdQueryHandler(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto?> HandleAsync(PureGetProductByIdQuery query, CancellationToken ctoken = default)
        {
            return await _context.Products.AsNoTracking()
                .Where(p => p.Id == query.Id)
                .Select(product => new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Sku = product.Sku,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    Category = product.Category,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                })
                .FirstOrDefaultAsync(ctoken);
        }
    }
}

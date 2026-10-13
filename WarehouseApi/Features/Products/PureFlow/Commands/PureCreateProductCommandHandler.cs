using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Commands
{
    public class PureCreateProductCommandHandler : ICommandHandler<PureCreateProductCommand, ProductDto>
    {
        private readonly WarehouseDbContext _context;

        public PureCreateProductCommandHandler(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto> HandleAsync(PureCreateProductCommand command, CancellationToken ctoken = default)
        {
            var product = new Product
            {
                Name = command.Name,
                Sku = command.Sku,
                Description = command.Description,
                Price = command.Price,
                StockQuantity = command.Stock,
                Category = command.Category
            };
            await _context.AddAsync(product);
            await _context.SaveChangesAsync(ctoken);
            return new ProductDto
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
            };
        }
    }
}

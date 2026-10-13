using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Products.Commands;

public class PureUpdateStockCommandHandler
    : ICommandHandler<PureUpdateStockCommand, ProductDto?>
{
    private readonly WarehouseDbContext _context;

    public PureUpdateStockCommandHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> HandleAsync(
        PureUpdateStockCommand command,
        CancellationToken token = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, token);

        if (product is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            throw new InvalidOperationException("Потрібно вказати причину зміни залишку.");
        }

        if (command.Reason.Length > 1000)
        {
            throw new InvalidOperationException("Причина не може перевищувати 1000 символів.");
        }

        int updatedQuantity;
        try
        {
            updatedQuantity = checked(product.StockQuantity + command.QuantityChange);
        }
        catch (OverflowException)
        {
            throw new InvalidOperationException("Кінцевий залишок виходить за допустимий діапазон.");
        }

        if (updatedQuantity < 0)
        {
            throw new InvalidOperationException("На складі недостатньо товару для такої зміни залишку.");
        }

        var timestamp = DateTime.UtcNow;
        product.StockQuantity = updatedQuantity;
        product.UpdatedAt = timestamp;

        _context.StockTransactions.Add(new StockTransaction
        {
            ProductId = product.Id,
            QuantityChange = command.QuantityChange,
            Reason = command.Reason.Trim(),
            Timestamp = timestamp
        });

        await _context.SaveChangesAsync(token);

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

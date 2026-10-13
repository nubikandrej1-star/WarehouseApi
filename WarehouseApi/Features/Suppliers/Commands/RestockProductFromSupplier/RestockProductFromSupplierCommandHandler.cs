using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.Features.Products.DTOs;

namespace WarehouseApi.Features.Suppliers.Commands;

public class RestockProductFromSupplierCommandHandler
    : ICommandHandler<RestockProductFromSupplierCommand, ProductDto>
{
    private readonly WarehouseDbContext _context;

    public RestockProductFromSupplierCommandHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> HandleAsync(
        RestockProductFromSupplierCommand command,
        CancellationToken token = default)
    {
        if (command.Quantity <= 0)
        {
            throw new InvalidOperationException("Кількість поставки повинна бути більшою за нуль.");
        }

        if (command.PurchasePrice < 0)
        {
            throw new InvalidOperationException("Закупівельна ціна не може бути від'ємною.");
        }

        if (string.IsNullOrWhiteSpace(command.InvoiceNumber) || command.InvoiceNumber.Trim().Length > 100)
        {
            throw new InvalidOperationException("Номер накладної є обов'язковим і не може перевищувати 100 символів.");
        }

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == command.SupplierId, token);

        if (supplier is null)
        {
            throw new KeyNotFoundException($"Постачальника з ID {command.SupplierId} не знайдено.");
        }

        if (!supplier.IsActive)
        {
            throw new InvalidOperationException("Не можна оформити поставку від неактивного постачальника.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, token);

        if (product is null)
        {
            throw new KeyNotFoundException($"Товар з ID {command.ProductId} не знайдено.");
        }

        int updatedQuantity;
        try
        {
            updatedQuantity = checked(product.StockQuantity + command.Quantity);
        }
        catch (OverflowException)
        {
            throw new InvalidOperationException("Кількість товару перевищує допустиме значення.");
        }

        var timestamp = DateTime.UtcNow;
        product.StockQuantity = updatedQuantity;
        product.UpdatedAt = timestamp;

        var reason = $"Поставка від '{supplier.CompanyName}' за накладною №{command.InvoiceNumber.Trim()} (ціна закупки: {command.PurchasePrice:C})";

        _context.StockTransactions.Add(new StockTransaction
        {
            ProductId = product.Id,
            QuantityChange = command.Quantity,
            Reason = reason,
            Timestamp = timestamp
        });

        // Одне SaveChanges зберігає і новий залишок, і запис аудиту в одній транзакції EF Core.
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

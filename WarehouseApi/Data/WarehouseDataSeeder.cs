using Microsoft.EntityFrameworkCore;
using WarehouseApi.Domain.Entities;

namespace WarehouseApi.Data;

public static class WarehouseDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WarehouseDbContext>>();

        await context.Database.MigrateAsync();

        if (await context.Products.AnyAsync())
        {
            return;
        }

        logger.LogInformation("Початок сідінгу товарів складу (Студентський проєкт)...");

        var products = new List<Product>
        {
            new()
            {
                Name = "Ноутбук Dell XPS 15",
                Sku = "LAP-DELL-001",
                Description = "Потужний робочий ноутбук з 15.6 4K OLED екраном та Intel Core i7",
                Price = 65000.00m,
                StockQuantity = 15,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Механічна клавіатура Keychron Q1 Pro",
                Sku = "KB-KEY-002",
                Description = "Бездротова кастомна механічна клавіатура з перемикачами Banana",
                Price = 7800.00m,
                StockQuantity = 30,
                Category = "Accessories",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Бездротова миша Logitech MX Master 3S",
                Sku = "MOU-LOG-003",
                Description = "Ергономічна безшумна миша для продуктивної роботи та розробки",
                Price = 4200.00m,
                StockQuantity = 45,
                Category = "Accessories",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Монітор LG UltraFine 27 4K",
                Sku = "MON-LG-004",
                Description = "IPS монітор з точним передаванням кольорів та підтримкою USB-C 90W",
                Price = 18500.00m,
                StockQuantity = 8,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Ергономічне крісло Herman Miller Aeron",
                Sku = "FUR-HM-005",
                Description = "Преміальне офісне ергономічне крісло розміру B",
                Price = 52000.00m,
                StockQuantity = 5,
                Category = "Furniture",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        foreach (var p in products)
        {
            context.StockTransactions.Add(new StockTransaction
            {
                ProductId = p.Id,
                QuantityChange = p.StockQuantity,
                Reason = "Початкове надходження на склад (Initial Stock)",
                Timestamp = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Успішно додано {Count} тестових товарів на склад", products.Count);
    }
}
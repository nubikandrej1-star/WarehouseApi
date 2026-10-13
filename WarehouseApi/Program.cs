using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using WarehouseApi.Common.Behaviors;
using WarehouseApi.Common.Middleware;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Features.Products.Commands;
using WarehouseApi.Features.Products.DTOs;
using WarehouseApi.Features.Products.Queries;
using WarehouseApi.Features.Suppliers.Commands;
using WarehouseApi.Features.Suppliers.DTOs;
using WarehouseApi.Features.Suppliers.Queries;
using WarehouseApi.Features.Shipments.Commands.CreateShipment;

var builder = WebApplication.CreateBuilder(args);

// 1. База даних MS SQL Server (LocalDB)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Рядок підключення 'DefaultConnection' не знайдено.");

builder.Services.AddDbContext<WarehouseDbContext>(options => options.UseSqlServer(connectionString));

// =========================================================================
// TODO (Зустріч 1): Реєстрація хендлерів для Pure CQRS (ICommandHandler, IQueryHandler)
// =========================================================================

builder.Services.AddScoped<ICommandHandler<PureCreateProductCommand, ProductDto>, PureCreateProductCommandHandler>();
builder.Services.AddScoped<IQueryHandler<PureGetProductByIdQuery, ProductDto?>, PureGetProductByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<PureGetProductsListQuery, List<ProductDto>>, PureGetProductsListQueryHandler>();
builder.Services.AddScoped<ICommandHandler<PureUpdateStockCommand, ProductDto?>, PureUpdateStockCommandHandler>();
builder.Services.AddScoped<ICommandHandler<PureDeleteProductCommand, bool>, PureDeleteProductCommandHandler>();

builder.Services.AddScoped<IQueryHandler<GetSuppliersListQuery, List<SupplierDto>>, GetSuppliersListQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetSupplierByIdQuery, SupplierDto?>, GetSupplierByIdQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateSupplierCommand, SupplierDto>, CreateSupplierCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RestockProductFromSupplierCommand, ProductDto>, RestockProductFromSupplierCommandHandler>();

// MediatR для Vertical Slice фіч та автоматична валідація всіх IRequest через pipeline.
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(CreateShipmentCommand).Assembly);
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(CreateShipmentCommand).Assembly);

builder.Services.AddControllers();

// 2. Налаштування OpenAPI та інтерактивної документації Scalar (.NET 10)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Warehouse API (Студентський проєкт - Модуль 15)",
            Version = "v1",
            Description = "Практична робота: архітектурний патерн CQRS, MediatR та Pipeline Behaviors"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// 3. Інтерфейс Scalar API Reference
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Warehouse CQRS API (.NET 10)").WithTheme(ScalarTheme.Moon);
    });
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

// 4. Автоматичні міграції та сідінг бази даних
try
{
    await WarehouseDataSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Помилка під час сідінгу бази даних");
}

app.Run();
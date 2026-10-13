using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.Features.Suppliers.DTOs;

namespace WarehouseApi.Features.Suppliers.Commands;

public class CreateSupplierCommandHandler
    : ICommandHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly WarehouseDbContext _context;

    public CreateSupplierCommandHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto> HandleAsync(
        CreateSupplierCommand command,
        CancellationToken token = default)
    {
        var companyName = command.CompanyName?.Trim();
        var edrpouCode = command.EdrpouCode?.Trim();

        if (string.IsNullOrWhiteSpace(companyName) || companyName.Length > 200)
        {
            throw new InvalidOperationException("Назва компанії обов'язкова та не може перевищувати 200 символів.");
        }

        if (string.IsNullOrWhiteSpace(edrpouCode) ||
            edrpouCode.Length is < 8 or > 10 ||
            edrpouCode.Any(c => !char.IsDigit(c)))
        {
            throw new InvalidOperationException("Код ЄДРПОУ / ІПН має містити від 8 до 10 цифр.");
        }

        if (await _context.Suppliers.AnyAsync(s => s.EdrpouCode == edrpouCode, token))
        {
            throw new DuplicateSupplierCodeException($"Постачальник із кодом '{edrpouCode}' уже існує.");
        }

        ValidateOptionalLength(command.ContactPerson, 100, "Контактна особа");
        ValidateOptionalLength(command.Phone, 20, "Телефон");
        ValidateOptionalLength(command.Email, 100, "Email");
        ValidateOptionalLength(command.Address, 500, "Адреса");

        var supplier = new Supplier
        {
            CompanyName = companyName,
            EdrpouCode = edrpouCode,
            ContactPerson = Normalize(command.ContactPerson),
            Phone = Normalize(command.Phone),
            Email = Normalize(command.Email),
            Address = Normalize(command.Address),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(token);

        return new SupplierDto
        {
            Id = supplier.Id,
            CompanyName = supplier.CompanyName,
            EdrpouCode = supplier.EdrpouCode,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        };
    }

    private static void ValidateOptionalLength(string? value, int maxLength, string fieldName)
    {
        if (value is not null && value.Trim().Length > maxLength)
        {
            throw new InvalidOperationException($"Поле '{fieldName}' не може перевищувати {maxLength} символів.");
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

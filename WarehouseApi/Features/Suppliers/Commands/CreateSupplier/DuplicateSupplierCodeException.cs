namespace WarehouseApi.Features.Suppliers.Commands;

public sealed class DuplicateSupplierCodeException : InvalidOperationException
{
    public DuplicateSupplierCodeException(string message) : base(message)
    {
    }
}

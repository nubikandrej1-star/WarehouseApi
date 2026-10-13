using Microsoft.EntityFrameworkCore;
using WarehouseApi.Common.PureCqrs;
using WarehouseApi.Data;

namespace WarehouseApi.Features.Products.Commands;

public class PureDeleteProductCommandHandler
    : ICommandHandler<PureDeleteProductCommand, bool>
{
    private readonly WarehouseDbContext _context;

    public PureDeleteProductCommandHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(
        PureDeleteProductCommand command,
        CancellationToken token = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, token);

        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(token);
        return true;
    }
}

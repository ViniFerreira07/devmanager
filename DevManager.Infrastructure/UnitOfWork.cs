using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;

namespace DevManager.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly DevManagerDbContext _context;

    public UnitOfWork(DevManagerDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

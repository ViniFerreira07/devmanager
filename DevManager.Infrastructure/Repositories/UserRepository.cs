using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(DevManagerDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Email == email, cancellationToken);
    }
}
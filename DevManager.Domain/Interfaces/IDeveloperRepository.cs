using DevManager.Domain.Entities;

namespace DevManager.Domain.Interfaces;

public interface IDeveloperRepository : IRepository<Developer>
{
    Task<Developer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(Guid excludeId, string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Developer>> GetPagedAsync(int page, int pageSize, string? name = null, Guid? cityId = null, Guid? languageId = null, string? seniority = null, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(string? name = null, Guid? cityId = null, Guid? languageId = null, string? seniority = null, CancellationToken cancellationToken = default);
}
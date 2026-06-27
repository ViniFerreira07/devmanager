using DevManager.Domain.Entities;

namespace DevManager.Domain.Interfaces;

public interface IProgrammingLanguageRepository : IRepository<ProgrammingLanguage>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid excludeId, string name, CancellationToken cancellationToken = default);
    Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default);
}
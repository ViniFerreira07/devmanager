using DevManager.Domain.Entities;

namespace DevManager.Domain.Interfaces;

public interface IStateRepository : IRepository<State>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid excludeId, string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUFAsync(string uf, CancellationToken cancellationToken = default);
}
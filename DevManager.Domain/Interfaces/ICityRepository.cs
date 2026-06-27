using DevManager.Domain.Entities;

namespace DevManager.Domain.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<bool> ExistsByNameAndStateAsync(string name, Guid stateId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndStateAsync(Guid excludeId, string name, Guid stateId, CancellationToken cancellationToken = default);
}
using DevManager.Application.Common;
using DevManager.Application.DTOs;

namespace DevManager.Application.Interfaces;

public interface IStateService
{
    Task<Result<IReadOnlyList<StateDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<StateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<StateDto>> CreateAsync(CreateStateRequest request, CancellationToken cancellationToken = default);
    Task<Result<StateDto>> UpdateAsync(Guid id, UpdateStateRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

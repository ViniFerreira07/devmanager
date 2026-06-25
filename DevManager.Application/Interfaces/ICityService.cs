using DevManager.Application.Common;
using DevManager.Application.DTOs;

namespace DevManager.Application.Interfaces;

public interface ICityService
{
    Task<Result<IReadOnlyList<CityDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CityDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CityDto>> CreateAsync(CreateCityRequest request, CancellationToken cancellationToken = default);
    Task<Result<CityDto>> UpdateAsync(Guid id, UpdateCityRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

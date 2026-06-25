using DevManager.Application.Common;
using DevManager.Application.DTOs;

namespace DevManager.Application.Interfaces;

public interface IDeveloperService
{
    Task<Result<PagedResult<DeveloperDto>>> GetPagedAsync(DeveloperFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Result<DeveloperDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DeveloperDto>> CreateAsync(CreateDeveloperRequest request, CancellationToken cancellationToken = default);
    Task<Result<DeveloperDto>> UpdateAsync(Guid id, UpdateDeveloperRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<byte[]>> GenerateReportPdfAsync(CancellationToken cancellationToken = default);
}

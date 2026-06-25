using DevManager.Application.Common;
using DevManager.Application.DTOs;

namespace DevManager.Application.Interfaces;

public interface IProgrammingLanguageService
{
    Task<Result<IReadOnlyList<ProgrammingLanguageDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ProgrammingLanguageDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProgrammingLanguageDto>> CreateAsync(CreateProgrammingLanguageRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProgrammingLanguageDto>> UpdateAsync(Guid id, UpdateProgrammingLanguageRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

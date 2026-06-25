using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;

namespace DevManager.Application.Services;

public class ProgrammingLanguageService : IProgrammingLanguageService
{
    private readonly IRepository<ProgrammingLanguage> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProgrammingLanguageRequest> _createValidator;

    public ProgrammingLanguageService(IRepository<ProgrammingLanguage> repository, IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateProgrammingLanguageRequest> createValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<IReadOnlyList<ProgrammingLanguageDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var languages = await _repository.ListAsync(cancellationToken);
        return Result<IReadOnlyList<ProgrammingLanguageDto>>.Ok(_mapper.Map<IReadOnlyList<ProgrammingLanguageDto>>(languages));
    }

    public async Task<Result<ProgrammingLanguageDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var language = await _repository.GetByIdAsync(id, cancellationToken);
        return language is null ? Result<ProgrammingLanguageDto>.Fail("Programming language not found.") : Result<ProgrammingLanguageDto>.Ok(_mapper.Map<ProgrammingLanguageDto>(language));
    }

    public async Task<Result<ProgrammingLanguageDto>> CreateAsync(CreateProgrammingLanguageRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ProgrammingLanguageDto>.Fail(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        }

        var language = _mapper.Map<ProgrammingLanguage>(request);
        await _repository.AddAsync(language, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProgrammingLanguageDto>.Ok(_mapper.Map<ProgrammingLanguageDto>(language), "Programming language created successfully.");
    }

    public async Task<Result<ProgrammingLanguageDto>> UpdateAsync(Guid id, UpdateProgrammingLanguageRequest request, CancellationToken cancellationToken = default)
    {
        var language = await _repository.GetByIdAsync(id, cancellationToken);
        if (language is null)
        {
            return Result<ProgrammingLanguageDto>.Fail("Programming language not found.");
        }

        _mapper.Map(request, language);
        language.Id = id;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProgrammingLanguageDto>.Ok(_mapper.Map<ProgrammingLanguageDto>(language), "Programming language updated successfully.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var language = await _repository.GetByIdAsync(id, cancellationToken);
        if (language is null)
        {
            return Result<bool>.Fail("Programming language not found.");
        }

        await _repository.DeleteAsync(language, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Programming language deleted successfully.");
    }
}

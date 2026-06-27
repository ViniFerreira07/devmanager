using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.Common.Exceptions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;

namespace DevManager.Application.Services;

public class ProgrammingLanguageService : IProgrammingLanguageService
{
    private readonly IProgrammingLanguageRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProgrammingLanguageRequest> _createValidator;

    public ProgrammingLanguageService(IProgrammingLanguageRepository repository, IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateProgrammingLanguageRequest> createValidator)
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
        return language is null
            ? Result<ProgrammingLanguageDto>.Fail("Linguagem não encontrada.")
            : Result<ProgrammingLanguageDto>.Ok(_mapper.Map<ProgrammingLanguageDto>(language));
    }

    public async Task<Result<ProgrammingLanguageDto>> CreateAsync(CreateProgrammingLanguageRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var exists = await _repository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            throw new ConflictException("LANGUAGE_ALREADY_EXISTS", "Esta linguagem já está cadastrada.");
        }

        var language = _mapper.Map<ProgrammingLanguage>(request);
        await _repository.AddAsync(language, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProgrammingLanguageDto>.Ok(_mapper.Map<ProgrammingLanguageDto>(language), "Linguagem criada com sucesso.");
    }

    public async Task<Result<ProgrammingLanguageDto>> UpdateAsync(Guid id, UpdateProgrammingLanguageRequest request, CancellationToken cancellationToken = default)
    {
        var language = await _repository.GetByIdAsync(id, cancellationToken);
        if (language is null)
        {
            throw new NotFoundException("PROGRAMMING_LANGUAGE", id);
        }

        var exists = await _repository.ExistsByNameAsync(id, request.Name, cancellationToken);
        if (exists)
        {
            throw new ConflictException("LANGUAGE_ALREADY_EXISTS", "Esta linguagem já está cadastrada.");
        }

        _mapper.Map(request, language);
        language.Id = id;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProgrammingLanguageDto>.Ok(_mapper.Map<ProgrammingLanguageDto>(language), "Linguagem atualizada com sucesso.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var language = await _repository.GetByIdAsync(id, cancellationToken);
        if (language is null)
        {
            throw new NotFoundException("PROGRAMMING_LANGUAGE", id);
        }

        if (await _repository.IsInUseAsync(id, cancellationToken))
        {
            throw new ConflictException("ENTITY_IN_USE", "Esta linguagem está sendo utilizada por desenvolvedores.");
        }

        await _repository.DeleteAsync(language, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Linguagem excluída com sucesso.");
    }
}
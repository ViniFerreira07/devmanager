using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.Common.Exceptions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;

namespace DevManager.Application.Services;

public class StateService : IStateService
{
    private readonly IStateRepository _stateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateStateRequest> _createValidator;

    public StateService(IStateRepository stateRepository, IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateStateRequest> createValidator)
    {
        _stateRepository = stateRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<IReadOnlyList<StateDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var states = await _stateRepository.ListAsync(cancellationToken);
        return Result<IReadOnlyList<StateDto>>.Ok(_mapper.Map<IReadOnlyList<StateDto>>(states));
    }

    public async Task<Result<StateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var state = await _stateRepository.GetByIdAsync(id, cancellationToken);
        return state is null
            ? Result<StateDto>.Fail("Estado não encontrado.")
            : Result<StateDto>.Ok(_mapper.Map<StateDto>(state));
    }

    public async Task<Result<StateDto>> CreateAsync(CreateStateRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        if (await _stateRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException("STATE_ALREADY_EXISTS", "Já existe um estado cadastrado com este nome.");
        }

        if (await _stateRepository.ExistsByUFAsync(request.UF, cancellationToken))
        {
            throw new ConflictException("STATE_UF_ALREADY_EXISTS", "Já existe um estado cadastrado com esta UF.");
        }

        var state = _mapper.Map<State>(request);
        await _stateRepository.AddAsync(state, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<StateDto>.Ok(_mapper.Map<StateDto>(state), "Estado criado com sucesso.");
    }

    public async Task<Result<StateDto>> UpdateAsync(Guid id, UpdateStateRequest request, CancellationToken cancellationToken = default)
    {
        var state = await _stateRepository.GetByIdAsync(id, cancellationToken);
        if (state is null)
        {
            throw new NotFoundException("STATE", id);
        }

        if (await _stateRepository.ExistsByNameAsync(id, request.Name, cancellationToken))
        {
            throw new ConflictException("STATE_ALREADY_EXISTS", "Já existe um estado cadastrado com este nome.");
        }

        _mapper.Map(request, state);
        state.Id = id;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<StateDto>.Ok(_mapper.Map<StateDto>(state), "Estado atualizado com sucesso.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var state = await _stateRepository.GetByIdAsync(id, cancellationToken);
        if (state is null)
        {
            throw new NotFoundException("STATE", id);
        }

        await _stateRepository.DeleteAsync(state, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Estado excluído com sucesso.");
    }
}
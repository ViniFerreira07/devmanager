using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Application.Services;

public class StateService : IStateService
{
    private readonly IRepository<State> _stateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateStateRequest> _createValidator;

    public StateService(IRepository<State> stateRepository, IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateStateRequest> createValidator)
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
        return state is null ? Result<StateDto>.Fail("State not found.") : Result<StateDto>.Ok(_mapper.Map<StateDto>(state));
    }

    public async Task<Result<StateDto>> CreateAsync(CreateStateRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<StateDto>.Fail(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        }

        var state = _mapper.Map<State>(request);
        await _stateRepository.AddAsync(state, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<StateDto>.Ok(_mapper.Map<StateDto>(state), "State created successfully.");
    }

    public async Task<Result<StateDto>> UpdateAsync(Guid id, UpdateStateRequest request, CancellationToken cancellationToken = default)
    {
        var state = await _stateRepository.GetByIdAsync(id, cancellationToken);
        if (state is null)
        {
            return Result<StateDto>.Fail("State not found.");
        }

        _mapper.Map(request, state);
        state.Id = id;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<StateDto>.Ok(_mapper.Map<StateDto>(state), "State updated successfully.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var state = await _stateRepository.GetByIdAsync(id, cancellationToken);
        if (state is null)
        {
            return Result<bool>.Fail("State not found.");
        }

        await _stateRepository.DeleteAsync(state, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "State deleted successfully.");
    }
}

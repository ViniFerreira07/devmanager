using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.Common.Exceptions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Application.Services;

public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;
    private readonly IStateRepository _stateRepository;
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCityRequest> _createValidator;

    public CityService(ICityRepository cityRepository, IStateRepository stateRepository, IAppDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateCityRequest> createValidator)
    {
        _cityRepository = cityRepository;
        _stateRepository = stateRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<IReadOnlyList<CityDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cities = await _context.Cities.AsNoTracking().Include(x => x.State).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<CityDto>>.Ok(_mapper.Map<IReadOnlyList<CityDto>>(cities));
    }

    public async Task<Result<CityDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var city = await _context.Cities.AsNoTracking().Include(x => x.State).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return city is null
            ? Result<CityDto>.Fail("Cidade não encontrada.")
            : Result<CityDto>.Ok(_mapper.Map<CityDto>(city));
    }

    public async Task<Result<CityDto>> CreateAsync(CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var state = await _stateRepository.GetByIdAsync(request.StateId, cancellationToken);
        if (state is null)
        {
            throw new NotFoundException("STATE", request.StateId);
        }

        if (await _cityRepository.ExistsByNameAndStateAsync(request.Name, request.StateId, cancellationToken))
        {
            throw new ConflictException("CITY_ALREADY_EXISTS", "Já existe esta cidade neste estado.");
        }

        var city = _mapper.Map<City>(request);
        await _cityRepository.AddAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(city.Id, cancellationToken);
    }

    public async Task<Result<CityDto>> UpdateAsync(Guid id, UpdateCityRequest request, CancellationToken cancellationToken = default)
    {
        var city = await _cityRepository.GetByIdAsync(id, cancellationToken);
        if (city is null)
        {
            throw new NotFoundException("CITY", id);
        }

        var state = await _stateRepository.GetByIdAsync(request.StateId, cancellationToken);
        if (state is null)
        {
            throw new NotFoundException("STATE", request.StateId);
        }

        if (await _cityRepository.ExistsByNameAndStateAsync(id, request.Name, request.StateId, cancellationToken))
        {
            throw new ConflictException("CITY_ALREADY_EXISTS", "Já existe esta cidade neste estado.");
        }

        _mapper.Map(request, city);
        city.Id = id;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var city = await _cityRepository.GetByIdAsync(id, cancellationToken);
        if (city is null)
        {
            throw new NotFoundException("CITY", id);
        }

        await _cityRepository.DeleteAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Cidade excluída com sucesso.");
    }
}
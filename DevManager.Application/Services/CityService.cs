using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Application.Services;

public class CityService : ICityService
{
    private readonly IRepository<City> _cityRepository;
    private readonly IRepository<State> _stateRepository;
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCityRequest> _createValidator;

    public CityService(IRepository<City> cityRepository, IRepository<State> stateRepository, IAppDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateCityRequest> createValidator)
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
        return city is null ? Result<CityDto>.Fail("City not found.") : Result<CityDto>.Ok(_mapper.Map<CityDto>(city));
    }

    public async Task<Result<CityDto>> CreateAsync(CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CityDto>.Fail(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        }

        var state = await _stateRepository.GetByIdAsync(request.StateId, cancellationToken);
        if (state is null)
        {
            return Result<CityDto>.Fail("State not found.");
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
            return Result<CityDto>.Fail("City not found.");
        }

        var state = await _stateRepository.GetByIdAsync(request.StateId, cancellationToken);
        if (state is null)
        {
            return Result<CityDto>.Fail("State not found.");
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
            return Result<bool>.Fail("City not found.");
        }

        await _cityRepository.DeleteAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "City deleted successfully.");
    }
}

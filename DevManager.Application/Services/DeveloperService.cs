using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DevManager.Application.Services;

public class DeveloperService : IDeveloperService
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateDeveloperRequest> _createValidator;

    public DeveloperService(IAppDbContext context, IMapper mapper, IValidator<CreateDeveloperRequest> createValidator)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<PagedResult<DeveloperDto>>> GetPagedAsync(DeveloperFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 10 : Math.Min(filter.PageSize, 100);
        var query = _context.Developers
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(x => x!.State)
            .Include(x => x.ProgrammingLanguages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (filter.CityId.HasValue)
        {
            query = query.Where(x => x.CityId == filter.CityId.Value);
        }

        if (filter.LanguageId.HasValue)
        {
            query = query.Where(x => x.ProgrammingLanguages.Any(p => p.Id == filter.LanguageId.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.Seniority))
        {
            var seniority = filter.Seniority.ToLower();
            query = query.Where(x => x.Seniority.ToLower().Contains(seniority));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<DeveloperDto>>.Ok(new PagedResult<DeveloperDto>
        {
            Items = _mapper.Map<IReadOnlyList<DeveloperDto>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<DeveloperDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var developer = await _context.Developers
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(x => x!.State)
            .Include(x => x.ProgrammingLanguages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return developer is null
            ? Result<DeveloperDto>.Fail("Developer not found.")
            : Result<DeveloperDto>.Ok(_mapper.Map<DeveloperDto>(developer));
    }

    public async Task<Result<DeveloperDto>> CreateAsync(CreateDeveloperRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<DeveloperDto>.Fail(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        }

        if (await _context.Developers.AnyAsync(x => x.Email == request.Email, cancellationToken))
        {
            return Result<DeveloperDto>.Fail("Developer email already registered.");
        }

        var cityExists = await _context.Cities.AnyAsync(x => x.Id == request.CityId, cancellationToken);
        if (!cityExists)
        {
            return Result<DeveloperDto>.Fail("City not found.");
        }

        var languageIds = request.ProgrammingLanguageIds.Distinct().ToArray();
        var selectedLanguages = await _context.ProgrammingLanguages
            .Where(x => languageIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (selectedLanguages.Count != languageIds.Length)
        {
            return Result<DeveloperDto>.Fail("One or more programming languages were not found.");
        }

        var developer = _mapper.Map<Developer>(request);
        developer.ProgrammingLanguages = selectedLanguages;
        await _context.Developers.AddAsync(developer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(developer.Id, cancellationToken);
    }

    public async Task<Result<DeveloperDto>> UpdateAsync(Guid id, UpdateDeveloperRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ProgrammingLanguageIds.Any())
        {
            return Result<DeveloperDto>.Fail("At least one programming language is required.");
        }

        var developer = await _context.Developers
            .Include(x => x.ProgrammingLanguages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (developer is null)
        {
            return Result<DeveloperDto>.Fail("Developer not found.");
        }

        if (await _context.Developers.AnyAsync(x => x.Id != id && x.Email == request.Email, cancellationToken))
        {
            return Result<DeveloperDto>.Fail("Developer email already registered.");
        }

        var cityExists = await _context.Cities.AnyAsync(x => x.Id == request.CityId, cancellationToken);
        if (!cityExists)
        {
            return Result<DeveloperDto>.Fail("City not found.");
        }

        var languageIds = request.ProgrammingLanguageIds.Distinct().ToArray();
        var selectedLanguages = await _context.ProgrammingLanguages.Where(x => languageIds.Contains(x.Id)).ToListAsync(cancellationToken);
        if (selectedLanguages.Count != languageIds.Length)
        {
            return Result<DeveloperDto>.Fail("One or more programming languages were not found.");
        }

        _mapper.Map(request, developer);
        developer.Id = id;
        developer.ProgrammingLanguages = selectedLanguages;
        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var developer = await _context.Developers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (developer is null)
        {
            return Result<bool>.Fail("Developer not found.");
        }

        developer.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Developer deleted successfully.");
    }

    public async Task<Result<byte[]>> GenerateReportPdfAsync(CancellationToken cancellationToken = default)
    {
        var developers = await _context.Developers
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(x => x!.State)
            .Include(x => x.ProgrammingLanguages)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Column(column =>
                {
                    column.Item().Text("DevManager - Developers Report").FontSize(20).Bold();
                    column.Item().Text($"Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[] { "Developer", "City", "State", "Languages", "Seniority" })
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text(title).FontColor(Colors.White).Bold();
                        }
                    });

                    foreach (var developer in developers)
                    {
                        table.Cell().Padding(5).Text(developer.Name);
                        table.Cell().Padding(5).Text(developer.City?.Name ?? "-");
                        table.Cell().Padding(5).Text(developer.City?.State?.Name ?? "-");
                        table.Cell().Padding(5).Text(string.Join(", ", developer.ProgrammingLanguages.Select(x => x.Name)));
                        table.Cell().Padding(5).Text(developer.Seniority);
                    }
                });
            });
        }).GeneratePdf();

        return Result<byte[]>.Ok(pdf);
    }
}

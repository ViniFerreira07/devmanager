using AutoMapper;
using DevManager.Application.Common;
using DevManager.Application.Common.Exceptions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DevManager.Application.Services;

public class DeveloperService : IDeveloperService
{
    private readonly IDeveloperRepository _developerRepository;
    private readonly IProgrammingLanguageRepository _languageRepository;
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateDeveloperRequest> _createValidator;

    public DeveloperService(
        IDeveloperRepository developerRepository,
        IProgrammingLanguageRepository languageRepository,
        IAppDbContext context,
        IMapper mapper,
        IValidator<CreateDeveloperRequest> createValidator)
    {
        _developerRepository = developerRepository;
        _languageRepository = languageRepository;
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<PagedResult<DeveloperDto>>> GetPagedAsync(DeveloperFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 10 : Math.Min(filter.PageSize, 100);

        var total = await _developerRepository.GetTotalCountAsync(
            filter.Name, filter.CityId, filter.LanguageId, filter.Seniority, cancellationToken);

        var items = await _developerRepository.GetPagedAsync(
            page, pageSize, filter.Name, filter.CityId, filter.LanguageId, filter.Seniority, cancellationToken);

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
            ? Result<DeveloperDto>.Fail("Desenvolvedor não encontrado.")
            : Result<DeveloperDto>.Ok(_mapper.Map<DeveloperDto>(developer));
    }

    public async Task<Result<DeveloperDto>> CreateAsync(CreateDeveloperRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        if (await _developerRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("DEVELOPER_EMAIL_ALREADY_EXISTS",
                "Já existe um desenvolvedor cadastrado com este e-mail.");
        }

        var cityExists = await _context.Cities.AnyAsync(x => x.Id == request.CityId, cancellationToken);
        if (!cityExists)
        {
            throw new NotFoundException("CITY", request.CityId);
        }

        var languageIds = request.ProgrammingLanguageIds.Distinct().ToArray();
        var selectedLanguages = await _context.ProgrammingLanguages
            .Where(x => languageIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (selectedLanguages.Count != languageIds.Length)
        {
            throw new NotFoundException("PROGRAMMING_LANGUAGE",
                "Uma ou mais linguagens de programação não foram encontradas.");
        }

        var developer = _mapper.Map<Developer>(request);
        developer.ProgrammingLanguages = selectedLanguages;

        var exists = (await _developerRepository.ListAsync(cancellationToken))
            .Any(d =>
                d.Email.Equals(request.Email,
                    StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            return Result<DeveloperDto>.Fail(
                "Já existe um desenvolvedor cadastrado com este e-mail.");
        }

        await _context.Developers.AddAsync(developer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(developer.Id, cancellationToken);
    }

    public async Task<Result<DeveloperDto>> UpdateAsync(Guid id, UpdateDeveloperRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ProgrammingLanguageIds.Any())
        {
            throw new Common.Exceptions.ValidationException("At least one programming language is required.");
        }

        var developer = await _context.Developers
            .Include(x => x.ProgrammingLanguages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (developer is null)
        {
            throw new NotFoundException("DEVELOPER", id);
        }

        if (await _developerRepository.ExistsByEmailAsync(id, request.Email, cancellationToken))
        {
            throw new ConflictException("DEVELOPER_EMAIL_ALREADY_EXISTS",
                "Já existe um desenvolvedor cadastrado com este e-mail.");
        }

        var cityExists = await _context.Cities.AnyAsync(x => x.Id == request.CityId, cancellationToken);
        if (!cityExists)
        {
            throw new NotFoundException("CITY", request.CityId);
        }

        var languageIds = request.ProgrammingLanguageIds.Distinct().ToArray();
        var selectedLanguages = await _context.ProgrammingLanguages.Where(x => languageIds.Contains(x.Id)).ToListAsync(cancellationToken);
        if (selectedLanguages.Count != languageIds.Length)
        {
            throw new NotFoundException("PROGRAMMING_LANGUAGE",
                "Uma ou mais linguagens de programação não foram encontradas.");
        }

        _mapper.Map(request, developer);
        developer.Id = id;
        developer.ProgrammingLanguages = selectedLanguages;

        var duplicated = (await _developerRepository.ListAsync(cancellationToken))
            .Any(d =>
                d.Id != id &&
                d.Email.Equals(request.Email,
                    StringComparison.OrdinalIgnoreCase));

        if (duplicated)
        {
            return Result<DeveloperDto>.Fail(
                "Já existe um desenvolvedor cadastrado com este e-mail.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var developer = await _context.Developers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (developer is null)
        {
            throw new NotFoundException("DEVELOPER", id);
        }

        developer.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Desenvolvedor excluído com sucesso.");
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
                    column.Item().Text("Relatório de Desenvolvedores").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
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
                        foreach (var title in new[] { "Desenvolvedor", "Cidade", "Estado", "Linguagens", "Senioridade" })
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

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber().FontSize(9);
                    text.Span(" / ").FontSize(9);
                    text.TotalPages().FontSize(9);
                });
            });
        }).GeneratePdf();

        return Result<byte[]>.Ok(pdf);
    }
}
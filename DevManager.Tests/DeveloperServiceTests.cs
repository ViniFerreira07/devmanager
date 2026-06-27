using AutoMapper;
using DevManager.Application.Common.Exceptions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Application.Mappings;
using DevManager.Application.Services;
using DevManager.Application.Validators;
using DevManager.Domain.Entities;
using DevManager.Domain.Enums;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace DevManager.Tests;

public class DeveloperServiceTests
{
    private static DevManagerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DevManagerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DevManagerDbContext(options);
    }

    private static MockValidator<CreateDeveloperRequest> CreateValidator() => new();

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenDeveloperHasNoLanguage()
    {
        await using var context = CreateContext();
        var city = await SeedCityAsync(context);
        var mapper = new MapperConfiguration(config => config.AddProfile<MappingProfile>()).CreateMapper();
        var service = new DeveloperService(
            new MockDeveloperRepository(context),
            new MockLanguageRepository(context),
            context,
            mapper,
            CreateValidator());

        var result = await service.CreateAsync(new CreateDeveloperRequest("Jane Doe", "jane@devmanager.com", "Senior", city.Id, null, Array.Empty<Guid>()));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("At least one programming language");
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        await using var context = CreateContext();
        var city = await SeedCityAsync(context);
        var language = await SeedLanguageAsync(context);
        var mapper = new MapperConfiguration(config => config.AddProfile<MappingProfile>()).CreateMapper();
        var service = new DeveloperService(
            new MockDeveloperRepository(context),
            new MockLanguageRepository(context),
            context,
            mapper,
            CreateValidator());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateDeveloperRequest("Jane Doe", "jane@devmanager.com", "Senior", city.Id, null, new[] { language.Id })));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateDeveloperRequest("Jane Other", "jane@devmanager.com", "Junior", city.Id, null, new[] { language.Id })));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteDeveloper()
    {
        await using var context = CreateContext();
        var city = await SeedCityAsync(context);
        var developer = new Developer { Name = "Jane Doe", Email = "jane@devmanager.com", Seniority = "Senior", CityId = city.Id };
        context.Developers.Add(developer);
        await context.SaveChangesAsync();

        var mapper = new MapperConfiguration(config => config.AddProfile<MappingProfile>()).CreateMapper();
        var service = new DeveloperService(
            new MockDeveloperRepository(context),
            new MockLanguageRepository(context),
            context,
            mapper,
            CreateValidator());

        var result = await service.DeleteAsync(developer.Id);

        result.Success.Should().BeTrue();
        (await context.Developers.CountAsync()).Should().Be(0);
        (await context.Developers.IgnoreQueryFilters().FirstAsync(x => x.Id == developer.Id)).DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnSuccessResult()
    {
        await using var context = CreateContext();
        await SeedCityAsync(context);
        var mapper = new MapperConfiguration(config => config.AddProfile<MappingProfile>()).CreateMapper();
        var service = new DeveloperService(
            new MockDeveloperRepository(context),
            new MockLanguageRepository(context),
            context,
            mapper,
            CreateValidator());

        var result = await service.GetPagedAsync(new DeveloperFilterRequest());

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    private static async Task<City> SeedCityAsync(DevManagerDbContext context)
    {
        var state = new State { Name = "Sao Paulo", UF = "SP" };
        var city = new City { Name = "Sao Paulo", State = state };
        context.States.Add(state);
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        return city;
    }

    private static async Task<ProgrammingLanguage> SeedLanguageAsync(DevManagerDbContext context)
    {
        var language = new ProgrammingLanguage { Name = "C#", Type = ProgrammingLanguageType.Backend };
        context.ProgrammingLanguages.Add(language);
        await context.SaveChangesAsync();
        return language;
    }

    private class MockDeveloperRepository : IDeveloperRepository
    {
        private readonly DevManagerDbContext _context;
        public MockDeveloperRepository(DevManagerDbContext context) => _context = context;

        public Task<Developer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Developers.FirstOrDefaultAsync(d => d.Id == id, ct)!;

        public Task<IReadOnlyList<Developer>> ListAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Developer>>(_context.Developers.ToList());

        public async Task<Developer> AddAsync(Developer entity, CancellationToken ct = default)
        {
            var result = await _context.Developers.AddAsync(entity, ct);
            return result.Entity;
        }

        public Task UpdateAsync(Developer entity, CancellationToken ct = default)
        {
            _context.Developers.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Developer entity, CancellationToken ct = default)
        {
            _context.Developers.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<Developer?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            _context.Developers.FirstOrDefaultAsync(d => d.Email == email, ct)!;

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
            _context.Developers.AnyAsync(d => d.Email == email, ct);

        public Task<bool> ExistsByEmailAsync(Guid excludeId, string email, CancellationToken ct = default) =>
            _context.Developers.AnyAsync(d => d.Id != excludeId && d.Email == email, ct);

        public async Task<IReadOnlyList<Developer>> GetPagedAsync(int page, int pageSize, string? name = null, Guid? cityId = null, Guid? languageId = null, string? seniority = null, CancellationToken ct = default)
        {
            var query = _context.Developers.AsNoTracking().Include(x => x.City).ThenInclude(x => x!.State).Include(x => x.ProgrammingLanguages).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.ToLower().Contains(name.ToLower()));
            if (cityId.HasValue) query = query.Where(x => x.CityId == cityId.Value);
            if (!string.IsNullOrWhiteSpace(seniority)) query = query.Where(x => x.Seniority.ToLower().Contains(seniority.ToLower()));
            return await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        }

        public async Task<int> GetTotalCountAsync(string? name = null, Guid? cityId = null, Guid? languageId = null, string? seniority = null, CancellationToken ct = default)
        {
            var query = _context.Developers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.ToLower().Contains(name.ToLower()));
            if (cityId.HasValue) query = query.Where(x => x.CityId == cityId.Value);
            if (!string.IsNullOrWhiteSpace(seniority)) query = query.Where(x => x.Seniority.ToLower().Contains(seniority.ToLower()));
            return await query.CountAsync(ct);
        }
    }

    private class MockLanguageRepository : IProgrammingLanguageRepository
    {
        private readonly DevManagerDbContext _context;
        public MockLanguageRepository(DevManagerDbContext context) => _context = context;

        public Task<ProgrammingLanguage?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.ProgrammingLanguages.FirstOrDefaultAsync(l => l.Id == id, ct)!;

        public Task<IReadOnlyList<ProgrammingLanguage>> ListAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ProgrammingLanguage>>(_context.ProgrammingLanguages.ToList());

        public async Task<ProgrammingLanguage> AddAsync(ProgrammingLanguage entity, CancellationToken ct = default)
        {
            var result = await _context.ProgrammingLanguages.AddAsync(entity, ct);
            return result.Entity;
        }

        public Task UpdateAsync(ProgrammingLanguage entity, CancellationToken ct = default)
        {
            _context.ProgrammingLanguages.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ProgrammingLanguage entity, CancellationToken ct = default)
        {
            _context.ProgrammingLanguages.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
            _context.ProgrammingLanguages.AnyAsync(l => l.Name.ToLower() == name.ToLower(), ct);

        public Task<bool> ExistsByNameAsync(Guid excludeId, string name, CancellationToken ct = default) =>
            _context.ProgrammingLanguages.AnyAsync(l => l.Id != excludeId && l.Name.ToLower() == name.ToLower(), ct);

        public Task<bool> IsInUseAsync(Guid id, CancellationToken ct = default) =>
            _context.DeveloperProgrammingLanguages.AnyAsync(d => d.ProgrammingLanguageId == id, ct);
    }

    private class MockValidator<T> : AbstractValidator<T>
    {
    }
}
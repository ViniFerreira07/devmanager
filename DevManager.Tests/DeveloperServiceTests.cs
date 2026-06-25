using AutoMapper;
using DevManager.Application.DTOs;
using DevManager.Application.Mappings;
using DevManager.Application.Services;
using DevManager.Application.Validators;
using DevManager.Domain.Entities;
using DevManager.Domain.Enums;
using DevManager.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Tests;

public class DeveloperServiceTests
{
    private static DeveloperService CreateService(DevManagerDbContext context)
    {
        var mapper = new MapperConfiguration(config => config.AddProfile<MappingProfile>()).CreateMapper();
        return new DeveloperService(context, mapper, new CreateDeveloperRequestValidator());
    }

    private static DevManagerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DevManagerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DevManagerDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenDeveloperHasNoLanguage()
    {
        await using var context = CreateContext();
        var city = await SeedCityAsync(context);
        var service = CreateService(context);

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
        var service = CreateService(context);

        await service.CreateAsync(new CreateDeveloperRequest("Jane Doe", "jane@devmanager.com", "Senior", city.Id, null, new[] { language.Id }));
        var duplicate = await service.CreateAsync(new CreateDeveloperRequest("Jane Other", "jane@devmanager.com", "Junior", city.Id, null, new[] { language.Id }));

        duplicate.Success.Should().BeFalse();
        duplicate.Message.Should().Contain("already registered");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteDeveloper()
    {
        await using var context = CreateContext();
        var city = await SeedCityAsync(context);
        var developer = new Developer { Name = "Jane Doe", Email = "jane@devmanager.com", Seniority = "Senior", CityId = city.Id };
        context.Developers.Add(developer);
        await context.SaveChangesAsync();

        var service = CreateService(context);
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
        var service = CreateService(context);

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
}

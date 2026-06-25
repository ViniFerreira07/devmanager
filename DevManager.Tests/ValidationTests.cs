using DevManager.Application.DTOs;
using DevManager.Application.Validators;
using DevManager.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DevManager.Tests;

public class ValidationTests
{
    [Fact]
    public async Task CreateDeveloperRequestValidator_ShouldFail_WhenEmailIsInvalid()
    {
        var validator = new CreateDeveloperRequestValidator();
        var request = new CreateDeveloperRequest("John Doe", "invalid-email", "Senior", Guid.NewGuid(), null, new[] { Guid.NewGuid() });

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task CreateDeveloperRequestValidator_ShouldFail_WhenNameTooShort()
    {
        var validator = new CreateDeveloperRequestValidator();
        var request = new CreateDeveloperRequest("Jo", "john@test.com", "Senior", Guid.NewGuid(), null, new[] { Guid.NewGuid() });

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateDeveloperRequestValidator_ShouldFail_WhenNoProgrammingLanguage()
    {
        var validator = new CreateDeveloperRequestValidator();
        var request = new CreateDeveloperRequest("John Doe", "john@test.com", "Senior", Guid.NewGuid(), null, Array.Empty<Guid>());

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProgrammingLanguageIds");
    }

    [Fact]
    public async Task CreateStateRequestValidator_ShouldPass_WithValidData()
    {
        var validator = new CreateStateRequestValidator();
        var request = new CreateStateRequest("Sao Paulo", "SP");

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateStateRequestValidator_ShouldFail_WhenUfNotTwoCharacters()
    {
        var validator = new CreateStateRequestValidator();
        var request = new CreateStateRequest("Sao Paulo", "SPP");

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCityRequestValidator_ShouldFail_WhenNameEmpty()
    {
        var validator = new CreateCityRequestValidator();
        var request = new CreateCityRequest("", Guid.NewGuid());

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateProgrammingLanguageRequestValidator_ShouldPass_WithValidData()
    {
        var validator = new CreateProgrammingLanguageRequestValidator();
        var request = new CreateProgrammingLanguageRequest("C#", ProgrammingLanguageType.Backend);

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }
}
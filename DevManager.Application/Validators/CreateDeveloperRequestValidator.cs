using DevManager.Application.DTOs;
using FluentValidation;

namespace DevManager.Application.Validators;

public class CreateDeveloperRequestValidator : AbstractValidator<CreateDeveloperRequest>
{
    public CreateDeveloperRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MinimumLength(3).MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().MaximumLength(200);
        RuleFor(x => x.Seniority).NotEmpty().WithMessage("Seniority is required.").MaximumLength(80);
        RuleFor(x => x.CityId).NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.ProgrammingLanguageIds).NotEmpty().WithMessage("At least one programming language is required.");
    }
}

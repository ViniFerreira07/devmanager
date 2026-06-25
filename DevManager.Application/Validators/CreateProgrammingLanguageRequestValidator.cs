using DevManager.Application.DTOs;
using FluentValidation;

namespace DevManager.Application.Validators;

public class CreateProgrammingLanguageRequestValidator : AbstractValidator<CreateProgrammingLanguageRequest>
{
    public CreateProgrammingLanguageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Programming language name is required.").MinimumLength(2).MaximumLength(150);
        RuleFor(x => x.Type).IsInEnum();
    }
}

using DevManager.Application.DTOs;
using FluentValidation;

namespace DevManager.Application.Validators;

public class CreateStateRequestValidator : AbstractValidator<CreateStateRequest>
{
    public CreateStateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("State name is required.").MinimumLength(2).MaximumLength(150);
        RuleFor(x => x.UF).NotEmpty().WithMessage("UF is required.").Length(2);
    }
}

using DevManager.Application.DTOs;
using FluentValidation;

namespace DevManager.Application.Validators;

public class CreateCityRequestValidator : AbstractValidator<CreateCityRequest>
{
    public CreateCityRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("City name is required.").MinimumLength(2).MaximumLength(150);
        RuleFor(x => x.StateId).NotEmpty().WithMessage("State is required.");
    }
}

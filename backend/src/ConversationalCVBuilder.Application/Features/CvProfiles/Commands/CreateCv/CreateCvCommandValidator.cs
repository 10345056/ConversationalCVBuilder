using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.CreateCv;

public sealed class CreateCvCommandValidator : AbstractValidator<CreateCvCommand>
{
    public CreateCvCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(120);
    }
}

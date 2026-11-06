using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.GenerateCv;

public sealed class GenerateCvCommandValidator : AbstractValidator<GenerateCvCommand>
{
    public GenerateCvCommandValidator()
    {
        RuleFor(command => command.CvProfileId).NotEmpty();
    }
}

using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Commands.UpdateCv;

public sealed class UpdateCvCommandValidator : AbstractValidator<UpdateCvCommand>
{
    public UpdateCvCommandValidator()
    {
        RuleFor(command => command.CvProfileId).NotEmpty();
        RuleFor(command => command)
            .Must(command =>
                command.Title is not null
                || command.TargetRole is not null
                || command.Summary is not null
                || command.Skills is not null
                || command.GivenName is not null
                || command.FamilyName is not null
                || command.Email is not null
                || command.Phone is not null
                || command.Location is not null)
            .WithMessage("At least one CV field must be provided.");
        RuleFor(command => command.Title).MaximumLength(120);
        RuleFor(command => command.TargetRole).MaximumLength(160);
        RuleFor(command => command.Summary).MaximumLength(5000);
        RuleFor(command => command.GivenName).MaximumLength(100);
        RuleFor(command => command.FamilyName).MaximumLength(100);
        RuleFor(command => command.Location).MaximumLength(300);
        RuleFor(command => command.Email)
            .EmailAddress()
            .When(command => !string.IsNullOrWhiteSpace(command.Email));
        RuleFor(command => command.Phone)
            .Matches(@"^[+0-9().\-\s]{7,30}$")
            .When(command => !string.IsNullOrWhiteSpace(command.Phone));
        RuleFor(command => command.Skills)
            .Must(skills => skills is null || skills.Count <= 100)
            .WithMessage("A CV cannot contain more than 100 skills.");
        RuleForEach(command => command.Skills)
            .NotEmpty()
            .MaximumLength(100);
    }
}

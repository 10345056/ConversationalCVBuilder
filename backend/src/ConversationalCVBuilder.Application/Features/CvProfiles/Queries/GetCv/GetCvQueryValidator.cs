using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.CvProfiles.Queries.GetCv;

public sealed class GetCvQueryValidator : AbstractValidator<GetCvQuery>
{
    public GetCvQueryValidator()
    {
        RuleFor(query => query.CvProfileId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("CV profile ID cannot be empty.");
    }
}

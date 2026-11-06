using FluentValidation;

namespace ConversationalCVBuilder.Application.Features.Chat.Queries.GetConversation;

public sealed class GetConversationQueryValidator : AbstractValidator<GetConversationQuery>
{
    public GetConversationQueryValidator()
    {
        RuleFor(query => query.SessionId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Conversation session ID cannot be empty.");
    }
}

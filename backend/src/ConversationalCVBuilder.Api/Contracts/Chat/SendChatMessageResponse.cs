using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Api.Contracts.Chat;

public sealed record SendChatMessageResponse(
    ConversationDto Conversation,
    CvProfileDto CvProfile,
    string CurrentStep);

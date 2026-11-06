using System.ComponentModel.DataAnnotations;

namespace ConversationalCVBuilder.Api.Contracts.Chat;

public sealed record SendChatMessageRequest(
    [param: Required]
    [param: StringLength(20_000, MinimumLength = 1)]
    string Message);

using ConversationalCVBuilder.Api.Contracts.Chat;
using ConversationalCVBuilder.Application.Features.Chat.Commands.SendMessage;
using ConversationalCVBuilder.Application.Features.Chat.Queries.GetConversation;
using ConversationalCVBuilder.Application.Features.CvProfiles.Queries.GetCv;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController(ISender sender) : ControllerBase
{
    [HttpPost("message")]
    [ProducesResponseType<SendChatMessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SendChatMessageResponse>> SendMessage(
        [FromBody] SendChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        var workspace = await sender.Send(
            new SendMessageCommand(request.Message),
            cancellationToken);
        var conversation = await sender.Send(
            new GetConversationQuery(workspace.Conversation.Id),
            cancellationToken);
        var cvProfile = await sender.Send(
            new GetCvQuery(workspace.Profile.Id),
            cancellationToken);

        return Ok(new SendChatMessageResponse(conversation, cvProfile, workspace.CurrentStep));
    }
}

using ConversationalCVBuilder.Api.Contracts.Cv;
using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.Chat.Commands.SendMessage;
using ConversationalCVBuilder.Application.Features.Chat.Queries.GetConversation;
using ConversationalCVBuilder.Application.Features.Templates.Commands.SelectTemplate;
using ConversationalCVBuilder.Application.Features.Templates.Queries.GetTemplates;
using ConversationalCVBuilder.Application.Services;
using ConversationalCVBuilder.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/workspace")]
public sealed class WorkspaceController(
    IWorkspaceRepository workspaceRepository,
    WorkspaceService workspaceService,
    IAiService aiService,
    ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Workspace>(StatusCodes.Status200OK)]
    public ActionResult<Workspace> Get() => Ok(workspaceRepository.Get());

    [HttpPost("new-session")]
    [ProducesResponseType<Workspace>(StatusCodes.Status200OK)]
    public ActionResult<Workspace> StartNewSession() =>
        Ok(workspaceRepository.StartNewSession());

    [HttpPost("messages")]
    [ProducesResponseType<Workspace>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Workspace>> SendMessage(
        [FromBody] MessageRequest request,
        CancellationToken cancellationToken)
    {
        var workspace = await sender.Send(new SendMessageCommand(request.Message), cancellationToken);
        return Ok(workspace);
    }

    [HttpPut("template")]
    [ProducesResponseType<Workspace>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Workspace>> SelectTemplate(
        [FromBody] TemplateRequest request,
        CancellationToken cancellationToken)
    {
        var workspace = await sender.Send(new SelectTemplateCommand(request.TemplateId), cancellationToken);
        return Ok(workspace);
    }

    [HttpPut("profile/manual")]
    [ProducesResponseType<Workspace>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<Workspace> SaveProfile(
        [FromBody] UpdateWorkspaceProfileRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        workspaceService.UpdateProfile(
            request.FullName,
            request.TargetRole,
            request.Email,
            request.Phone,
            request.Location,
            request.LinkedIn,
            request.Summary,
            request.Skills,
            request.Experience,
            request.Education,
            request.Certifications,
            request.Projects,
            request.Languages,
            request.Achievements,
            request.AdditionalInformation);
        workspaceService.SetAiInsights("manual", null);
        return Ok(workspaceRepository.Get());
    }

    [HttpPut("profile")]
    [ProducesResponseType<Workspace>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Workspace>> UpdateProfile(
        [FromBody] UpdateWorkspaceProfileRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        workspaceService.UpdateProfile(
            request.FullName,
            request.TargetRole,
            request.Email,
            request.Phone,
            request.Location,
            request.LinkedIn,
            request.Summary,
            request.Skills,
            request.Experience,
            request.Education,
            request.Certifications,
            request.Projects,
            request.Languages,
            request.Achievements,
            request.AdditionalInformation);
        var currentWorkspace = workspaceService.Get();
        var conversation = currentWorkspace.Messages
            .Select(message => new AiConversationMessage(message.Role, message.Content))
            .ToArray();
        var result = await aiService.OptimizeCvAsync(
            workspaceService.GetAiProfile(),
            currentWorkspace.SourceText,
            conversation,
            cancellationToken);
        if (result.Value is { } analysis && !string.IsNullOrWhiteSpace(analysis.Profile.FullName))
        {
            workspaceService.UpdateProfile(analysis.Profile.WithFallback(workspaceService.GetAiProfile()));
            workspaceService.SetAiInsights(
                "optimized",
                null,
                analysis.MissingFields,
                analysis.NextQuestion,
                analysis.SuggestedImprovements,
                analysis.TemplateRecommendation);
            workspaceService.SelectTemplate(analysis.TemplateRecommendation);
        }
        else
        {
            var error = result.Error
                ?? "Some profile details could not be confirmed. Your information is preserved for review.";
            workspaceService.SetAiInsights("fallback", error);
        }

        return Ok(workspaceRepository.Get());
    }

    [HttpGet("conversation")]
    public async Task<IActionResult> GetConversation(CancellationToken cancellationToken)
    {
        var conversation = await sender.Send(new GetConversationQuery(), cancellationToken);
        return Ok(conversation);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await sender.Send(new GetTemplatesQuery(), cancellationToken);
        return Ok(templates);
    }
}

public sealed record MessageRequest(string Message);
public sealed record TemplateRequest(string TemplateId);

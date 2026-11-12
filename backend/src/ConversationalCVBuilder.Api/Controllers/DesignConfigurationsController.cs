using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Contracts;
using ConversationalCVBuilder.Application.Features.Exports.Commands.ExportCv;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ConversationalCVBuilder.Api.Controllers;

[ApiController]
[Route("api/design-configurations")]
public sealed class DesignConfigurationsController(
    ICvDesignPresetStore presetStore,
    IValidator<CvDesignConfiguration> validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CvDesignConfiguration>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CvDesignConfiguration>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await presetStore.GetAllAsync(cancellationToken));

    [HttpPut]
    [ProducesResponseType<CvDesignConfiguration>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CvDesignConfiguration>> Save(
        [FromBody] CvDesignConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(configuration.Id, out _))
        {
            ModelState.AddModelError(nameof(configuration.Id), "Configuration ID must be a GUID.");
            return ValidationProblem(ModelState);
        }

        var validation = await validator.ValidateAsync(configuration, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        return Ok(await presetStore.SaveAsync(configuration, cancellationToken));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out _)
            || !await presetStore.DeleteAsync(id, cancellationToken))
        {
            return NotFound();
        }

        return NoContent();
    }
}

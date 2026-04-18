using FinalProject_SeventhSem.Application.Features.Vacancies.Commands.CreateVacancy;
using FinalProject_SeventhSem.Application.Features.Vacancies.Commands.PublishVacancy;
using FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancyById;
using FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancyMatches;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

/// <summary>
/// Vacancy creation and publishing (Organization) + vacancy matching for students (Algorithms 3–6).
/// </summary>
public class VacanciesController : ApiController
{
    private int CurrentUserId =>
      int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Student endpoints ─────────────────────────────────────────────────

    /// <summary>
    /// Get all published vacancies with full match scores for the authenticated student.
    /// Runs Algorithms 3–6. Sorted: RequirementFit → OptionalFit → EducationBonus DESC.
    /// Ineligible vacancies are included with IsEligible = false.
    /// </summary>
    [HttpGet("matches")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<VacancyMatchResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMatches(CancellationToken ct)
    {
        var result = await Sender.Send(new GetVacancyMatchesQuery(CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Get details of a specific vacancy.</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetVacancyByIdQuery(id), ct);
        return Ok(result);
    }

    // ── Organization endpoints ─────────────────────────────────────────────

    /// <summary>Create a new vacancy draft (not yet published).</summary>
    [HttpPost]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVacancyRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new CreateVacancyCommand(
            OrganizationId: CurrentUserId,
            Title: request.Title,
            Description: request.Description,
            RequiredEducationLevel: request.RequiredEducationLevel,
            RequiredFieldOfStudy: request.RequiredFieldOfStudy,
            RequiredSkillIds: request.RequiredSkillIds,
            OptionalSkillIds: request.OptionalSkillIds), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.VacancyId }, result);
    }

    /// <summary>
    /// Publish a vacancy draft. Backend guard: must have at least 1 required skill → 400 if not.
    /// </summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        await Sender.Send(new PublishVacancyCommand(id, CurrentUserId), ct);
        return NoContent();
    }
}

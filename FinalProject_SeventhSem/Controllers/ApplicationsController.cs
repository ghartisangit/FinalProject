using FinalProject_SeventhSem.Application.Features.Applications.Commands.ApplyToVacancy;
using FinalProject_SeventhSem.Application.Features.Applications.Commands.UpdateApplicationStatus;
using FinalProject_SeventhSem.Application.Features.Applications.Queries.GetRankedCandidates;
using FinalProject_SeventhSem.Application.Models.Applications;
using FinalProject_SeventhSem.Application.Models.Ranking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;


/// <summary>
/// Student applications and organization-side candidate ranking (Algorithm 12).
/// </summary>


public class ApplicationsController : ApiController
{
    private int CurrentUserId =>
       int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Student endpoints ─────────────────────────────────────────────────

    /// <summary>
    /// Apply to a published vacancy. Captures an immutable ApplicationMatchSnapshot
    /// using Algorithms 3–6 at the moment of application.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Apply(
        [FromBody] ApplyToVacancyRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new ApplyToVacancyCommand(CurrentUserId, request.VacancyId), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── Organization endpoints ─────────────────────────────────────────────

    /// <summary>
    /// Get ranked candidates for a vacancy. Applies eligibility filter then Algorithm 12.
    /// Only the owning Organization may access this.
    /// </summary>
    [HttpGet("vacancies/{vacancyId:int}/ranked")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(RankedCandidateListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRankedCandidates(int vacancyId, CancellationToken ct)
    {
        var result = await Sender.Send(
            new GetRankedCandidatesQuery(vacancyId, CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Update an application's status.
    /// Status flow: Applied → UnderReview → Shortlisted → Rejected / Offered.
    /// Only the Organization that owns the vacancy may call this.
    /// </summary>
    [HttpPatch("{applicationId:int}/status")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int applicationId,
        [FromBody] UpdateApplicationStatusRequest request,
        CancellationToken ct)
    {
        await Sender.Send(new UpdateApplicationStatusCommand(
            applicationId, CurrentUserId, request.NewStatus), ct);
        return NoContent();
    }
}

using FinalProject_SeventhSem.Application.Features.Applications.Commands.ApplyToVacancy;
using FinalProject_SeventhSem.Application.Features.Applications.Commands.UpdateApplicationStatus;
using FinalProject_SeventhSem.Application.Features.Applications.Queries.GetRankedCandidates;
using FinalProject_SeventhSem.Application.Features.Applications.Queries.GetStudentApplications;
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

    // ── Student ────────────────────────────────────────────────────────────

    /// <summary>Apply to a published vacancy. Captures immutable match snapshot (Algorithms 3–6).</summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Apply(
        [FromBody] ApplyToVacancyRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(
            new ApplyToVacancyCommand(CurrentUserId, request.VacancyId), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Get all applications submitted by the authenticated student.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApplications(CancellationToken ct)
    {
        var result = await Sender.Send(new GetStudentApplicationsQuery(CurrentUserId), ct);
        return Ok(result);
    }

    // ── Organization ───────────────────────────────────────────────────────

    /// <summary>Get ranked candidates for a vacancy — Algorithm 12. Owner org only.</summary>
    [HttpGet("vacancies/{vacancyId:int}/ranked")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(RankedCandidateListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRankedCandidates(int vacancyId, CancellationToken ct)
    {
        var result = await Sender.Send(
            new GetRankedCandidatesQuery(vacancyId, CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Update application status. Only the owning Organization may call this.</summary>
    [HttpPatch("{applicationId:int}/status")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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

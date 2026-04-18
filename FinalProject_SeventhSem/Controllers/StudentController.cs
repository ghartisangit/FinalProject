using FinalProject_SeventhSem.Application.Features.Students.Commands.SetStudentSkills;
using FinalProject_SeventhSem.Application.Features.Students.Commands.UpdateStudentProfile;
using FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentDashboard;
using FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentProfile;
using FinalProject_SeventhSem.Application.Models.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

/// <summary>
/// Student profile management and dashboard.
/// All endpoints require the Student role.
/// </summary>
[Authorize(Roles = "Student")]
public class StudentController : ApiController
{
    private int CurrentStudentId =>
       int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get the authenticated student's full profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(StudentProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await Sender.Send(new GetStudentProfileQuery(CurrentStudentId), ct);
        return Ok(result);
    }

    /// <summary>Update the authenticated student's profile fields.</summary>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateStudentProfileRequest request,
        CancellationToken ct)
    {
        await Sender.Send(new UpdateStudentProfileCommand(
            StudentId: CurrentStudentId,
            FullName: request.FullName,
            PhoneNumber: request.PhoneNumber,
            Bio: request.Bio,
            Nationality: request.Nationality,
            Location: request.Location,
            EducationLevel: request.EducationLevel,
            FieldOfStudy: request.FieldOfStudy,
            GitHubUrl: request.GitHubUrl,
            PortfolioUrl: request.PortfolioUrl,
            LinkedInUrl: request.LinkedInUrl), ct);

        return NoContent();
    }

    /// <summary>
    /// Get the authenticated student's profile completeness dashboard.
    /// Computed on the fly — max score 100.
    /// </summary>
    [HttpGet("me/dashboard")]
    [ProducesResponseType(typeof(ProfileCompletenessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await Sender.Send(new GetStudentDashboardQuery(CurrentStudentId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Replace the student's confirmed skill set.
    /// Called after reviewing resume parsing suggestions.
    /// </summary>
    [HttpPut("me/skills")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetSkills(
        [FromBody] IReadOnlyList<int> confirmedSkillIds,
        CancellationToken ct)
    {
        await Sender.Send(new SetStudentSkillsCommand(CurrentStudentId, confirmedSkillIds), ct);
        return NoContent();
    }
}

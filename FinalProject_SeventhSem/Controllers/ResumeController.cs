using FinalProject_SeventhSem.Application.Features.Resume.Commands.UploadResume;
using FinalProject_SeventhSem.Application.Features.Students.Commands.SetStudentSkills;
using FinalProject_SeventhSem.Application.Models.Resume;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

/// <summary>
/// Resume upload (PDF only) and skill confirmation.
/// Rate-limited to 5 uploads/hour via AspNetCoreRateLimit (configured in Program.cs).
/// </summary>
[Authorize(Roles = "Student")]
public class ResumeController : ApiController
{
    private int CurrentStudentId =>
       int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Upload a PDF resume. Returns skill suggestions from Algorithm 2.
    /// Student must call POST /confirm after reviewing suggestions.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ResumeParseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        await using var stream = file.OpenReadStream();

        var result = await Sender.Send(
            new UploadResumeCommand(CurrentStudentId, stream, file.FileName), ct);

        return Ok(result);
    }

    /// <summary>
    /// Confirm the final list of skills after reviewing suggestions.
    /// Replaces all previous confirmed skills for this student.
    /// </summary>
    [HttpPost("confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmSkills(
        [FromBody] ConfirmResumeSkillsRequest request,
        CancellationToken ct)
    {
        await Sender.Send(
            new SetStudentSkillsCommand(CurrentStudentId, request.ConfirmedSkillIds), ct);

        return NoContent();
    }
}

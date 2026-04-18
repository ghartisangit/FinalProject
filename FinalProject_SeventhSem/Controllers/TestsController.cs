using FinalProject_SeventhSem.Application.Features.Tests.Commands.StartTest;
using FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitAnswer;
using FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitTest;
using FinalProject_SeventhSem.Application.Models.Tests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

/// <summary>
/// Aptitude test lifecycle: start → answer → submit.
///
/// Start    → Algorithm 7  (Round-Robin Question Selection)
/// Submit   → Algorithm 8  (Test Scoring)
///          + Algorithm 9  (Chapter Analysis)
///          + Algorithm 10 (Weak Chapter Detection)
///          + Algorithm 11 (Resource Recommendation)
///
/// Rate-limited: 3 submissions/hour via AspNetCoreRateLimit.
/// Time limit enforced per-answer: auto-submits if ExpiresAt exceeded.
/// </summary>
[Authorize(Roles = "Student")]
public class TestsController : ApiController
{
    private int CurrentStudentId =>
       int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Start a new test session. Returns question set selected by Algorithm 7.
    /// Tests are general — not tied to any vacancy.
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(TestSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start(CancellationToken ct)
    {
        var result = await Sender.Send(new StartTestCommand(CurrentStudentId), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Submit a single answer for an active test session.
    /// Checks time limit on every call; auto-submits and throws 400 if expired.
    /// </summary>
    [HttpPost("{testId:int}/answers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAnswer(
        int testId,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken ct)
    {
        await Sender.Send(new SubmitAnswerCommand(
            testId, CurrentStudentId, request.QuestionId, request.SelectedOption), ct);
        return NoContent();
    }

    /// <summary>
    /// Explicitly submit a test. Triggers Algorithms 8–11 and returns the full result.
    /// Only answered questions are scored.
    /// </summary>
    [HttpPost("{testId:int}/submit")]
    [ProducesResponseType(typeof(TestResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(int testId, CancellationToken ct)
    {
        var result = await Sender.Send(new SubmitTestCommand(testId, CurrentStudentId), ct);
        return Ok(result);
    }
}

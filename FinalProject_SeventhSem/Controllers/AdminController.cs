using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageQuestion;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageSkill;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.VerifyOrganization;
using FinalProject_SeventhSem.Application.Models.Skills;
using FinalProject_SeventhSem.Application.Models.Stacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace FinalProject_SeventhSem.Controllers;

/// <summary>
/// Admin-only operations:
///   - Verify organizations
///   - Manage skills and skill aliases
///   - Manage stacks, chapters, and questions
/// All endpoints require the Admin role.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : ApiController
{
    // ── Organizations ─────────────────────────────────────────────────────

    /// <summary>
    /// Verify an organization account so it can log in and post vacancies.
    /// </summary>
    [HttpPost("organizations/{organizationId:int}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyOrganization(int organizationId, CancellationToken ct)
    {
        await Sender.Send(new VerifyOrganizationCommand(organizationId), ct);
        return NoContent();
    }

    // ── Skills ────────────────────────────────────────────────────────────

    /// <summary>Create a new canonical skill.</summary>
    [HttpPost("skills")]
    [ProducesResponseType(typeof(SkillResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSkill(
        [FromBody] CreateSkillRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new CreateSkillCommand(request.Name), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Create a skill alias.
    /// Guards enforced: length >= 2, not a stopword, not mapped to another SkillId.
    /// </summary>
    [HttpPost("skills/aliases")]
    [ProducesResponseType(typeof(SkillAliasResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSkillAlias(
        [FromBody] CreateSkillAliasRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new CreateSkillAliasCommand(request.SkillId, request.Alias), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── Questions ─────────────────────────────────────────────────────────

    /// <summary>Add a multiple-choice question to a chapter.</summary>
    [HttpPost("questions")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateQuestion(
        [FromBody] CreateQuestionRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new CreateQuestionCommand(
            request.ChapterId, request.Text,
            request.OptionA, request.OptionB,
            request.OptionC, request.OptionD,
            request.CorrectOption), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Delete a question by ID.</summary>
    [HttpDelete("questions/{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(int questionId, CancellationToken ct)
    {
        await Sender.Send(new DeleteQuestionCommand(questionId), ct);
        return NoContent();
    }
}

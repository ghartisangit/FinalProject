using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageChapter;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageQuestion;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageResource;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageSkill;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageStack;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.VerifyOrganization;
using FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAllOrganizations;
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
    // ── Organizations ──────────────────────────────────────────────────────

    /// <summary>List all organizations. Use ?pendingOnly=true to filter unverified.</summary>
    [HttpGet("organizations")]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] bool pendingOnly = false, CancellationToken ct = default)
        => Ok(await Sender.Send(new GetAllOrganizationsQuery(pendingOnly), ct));

    /// <summary>Verify an organization account so it can log in.</summary>
    [HttpPost("organizations/{organizationId:int}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VerifyOrganization(int organizationId, CancellationToken ct)
    {
        await Sender.Send(new VerifyOrganizationCommand(organizationId), ct);
        return NoContent();
    }

    // ── Skills ─────────────────────────────────────────────────────────────

    /// <summary>Create a canonical skill.</summary>
    [HttpPost("skills")]
    [ProducesResponseType(typeof(SkillResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSkill(
        [FromBody] CreateSkillRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateSkillCommand(request.Name), ct));

    /// <summary>Create a skill alias. Guards: length ≥ 2, not a stopword, unique mapping.</summary>
    [HttpPost("skills/aliases")]
    [ProducesResponseType(typeof(SkillAliasResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSkillAlias(
        [FromBody] CreateSkillAliasRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateSkillAliasCommand(request.SkillId, request.Alias), ct));

    /// <summary>Delete a skill alias by ID.</summary>
    [HttpDelete("skills/aliases/{aliasId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSkillAlias(int aliasId, CancellationToken ct)
    {
        await Sender.Send(new DeleteSkillAliasCommand(aliasId), ct);
        return NoContent();
    }

    // ── Stacks ─────────────────────────────────────────────────────────────

    /// <summary>Get all stacks with chapter and question counts.</summary>
    [HttpGet("stacks")]
    [ProducesResponseType(typeof(IReadOnlyList<StackResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStacks(CancellationToken ct)
        => Ok(await Sender.Send(new GetAllStacksQuery(), ct));

    /// <summary>Create a new stack (e.g. ".NET", "Python").</summary>
    [HttpPost("stacks")]
    [ProducesResponseType(typeof(StackResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateStack(
        [FromBody] CreateStackRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateStackCommand(request.Name), ct));

    /// <summary>Update a stack name.</summary>
    [HttpPut("stacks/{stackId:int}")]
    [ProducesResponseType(typeof(StackResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStack(
        int stackId, [FromBody] UpdateStackRequest request, CancellationToken ct)
        => Ok(await Sender.Send(new UpdateStackCommand(stackId, request.Name), ct));

    /// <summary>Delete a stack (cascades to chapters and questions).</summary>
    [HttpDelete("stacks/{stackId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteStack(int stackId, CancellationToken ct)
    {
        await Sender.Send(new DeleteStackCommand(stackId), ct);
        return NoContent();
    }

    // ── Chapters ───────────────────────────────────────────────────────────

    /// <summary>Get all chapters for a stack.</summary>
    [HttpGet("stacks/{stackId:int}/chapters")]
    [ProducesResponseType(typeof(IReadOnlyList<ChapterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChapters(int stackId, CancellationToken ct)
        => Ok(await Sender.Send(new GetChaptersByStackQuery(stackId), ct));

    /// <summary>Create a chapter inside a stack.</summary>
    [HttpPost("chapters")]
    [ProducesResponseType(typeof(ChapterResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChapter(
        [FromBody] CreateChapterRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateChapterCommand(request.StackId, request.Name), ct));

    /// <summary>Update a chapter name.</summary>
    [HttpPut("chapters/{chapterId:int}")]
    [ProducesResponseType(typeof(ChapterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateChapter(
        int chapterId, [FromBody] UpdateChapterRequest request, CancellationToken ct)
        => Ok(await Sender.Send(new UpdateChapterCommand(chapterId, request.Name), ct));

    /// <summary>Delete a chapter (cascades to questions).</summary>
    [HttpDelete("chapters/{chapterId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteChapter(int chapterId, CancellationToken ct)
    {
        await Sender.Send(new DeleteChapterCommand(chapterId), ct);
        return NoContent();
    }

    // ── Questions ──────────────────────────────────────────────────────────

    /// <summary>Add a multiple-choice question to a chapter.</summary>
    [HttpPost("questions")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateQuestion(
        [FromBody] CreateQuestionRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateQuestionCommand(
                request.ChapterId, request.Text, request.OptionA, request.OptionB,
                request.OptionC, request.OptionD, request.CorrectOption), ct));

    /// <summary>Delete a question by ID.</summary>
    [HttpDelete("questions/{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteQuestion(int questionId, CancellationToken ct)
    {
        await Sender.Send(new DeleteQuestionCommand(questionId), ct);
        return NoContent();
    }

    // ── Resources ──────────────────────────────────────────────────────────

    /// <summary>Get all learning resources with skill mappings and rating averages.</summary>
    [HttpGet("resources")]
    [ProducesResponseType(typeof(IReadOnlyList<ResourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResources(CancellationToken ct)
        => Ok(await Sender.Send(new GetAllResourcesQuery(), ct));

    /// <summary>Create a learning resource and link it to skills.</summary>
    [HttpPost("resources")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateResource(
        [FromBody] CreateResourceCommand request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(request, ct));

    /// <summary>Update a resource's details and skill mappings.</summary>
    [HttpPut("resources/{resourceId:int}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateResource(
        int resourceId, [FromBody] UpdateResourceCommand request, CancellationToken ct)
        => Ok(await Sender.Send(request with { ResourceId = resourceId }, ct));

    /// <summary>Delete a resource by ID.</summary>
    [HttpDelete("resources/{resourceId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteResource(int resourceId, CancellationToken ct)
    {
        await Sender.Send(new DeleteResourceCommand(resourceId), ct);
        return NoContent();
    }
}

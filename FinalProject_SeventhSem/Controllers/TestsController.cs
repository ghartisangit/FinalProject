using FinalProject_SeventhSem.Application.Features.Tests.Commands.StartTest;
using FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitAnswer;
using FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitTest;
using FinalProject_SeventhSem.Application.Features.Tests.Queries.GetTestHistory;
using FinalProject_SeventhSem.Application.Models.Tests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;


[Authorize(Roles = "Student")]
public class TestsController : ApiController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    [HttpPost("start")]
    [ProducesResponseType(typeof(TestSessionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Start([FromBody] StartTestRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new StartTestCommand(CurrentUserId, request.StackId), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

 
    [HttpPost("{testId:int}/answers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SubmitAnswer(
        int testId, [FromBody] SubmitAnswerRequest request, CancellationToken ct)
    {
        await Sender.Send(new SubmitAnswerCommand(
            testId, CurrentUserId, request.QuestionId, request.SelectedOption), ct);
        return NoContent();
    }

 
    [HttpPost("{testId:int}/submit")]
    [ProducesResponseType(typeof(TestResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(int testId, CancellationToken ct)
    {
        var result = await Sender.Send(new SubmitTestCommand(testId, CurrentUserId), ct);
        return Ok(result);
    }


    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<TestHistoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var result = await Sender.Send(new GetTestHistoryQuery(CurrentUserId), ct);
        return Ok(result);
    }
}

public record StartTestRequest(int StackId);

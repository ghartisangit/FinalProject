using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Tests;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitTest;

/// <summary>
/// Algorithms 8–11:
///   8  — Test Scoring         : Score = (Correct / TotalAnswered) * 100
///   9  — Chapter Analysis     : group answers by chapter, compute per-chapter %
///   10 — Weak Chapter         : ChapterScore &lt; WeakChapterMaxPercent → weak
///   11 — Resource Recommendation : weak chapters + missing skills → resources
///
/// Also:
///   - Bulk-inserts StudentSeenQuestions.
///   - Sets IsLatest = false on all previous TestResults for this student.
///   - Auto-submits if called after ExpiresAt (only answered questions scored).
/// </summary>
public class SubmitTestCommandHandler : IRequestHandler<SubmitTestCommand, TestResultResponse>
{
    private readonly IRepository<Test> _testRepo;
    private readonly IRepository<TestAnswer> _answerRepo;
    private readonly IRepository<TestResult> _testResultRepo;
    private readonly IRepository<StudentSeenQuestion> _seenRepo;
    private readonly IRepository<Resource> _resourceRepo;
    private readonly IRepository<Chapter> _chapterRepo;
    private readonly IUnitOfWork _uow;
    private readonly ThresholdSettings _thresholds;

    public SubmitTestCommandHandler(
        IRepository<Test> testRepo,
        IRepository<TestAnswer> answerRepo,
        IRepository<TestResult> testResultRepo,
        IRepository<StudentSeenQuestion> seenRepo,
        IRepository<Resource> resourceRepo,
        IRepository<Chapter> chapterRepo,
        IUnitOfWork uow,
        IOptions<ThresholdSettings> thresholds)
    {
        _testRepo = testRepo;
        _answerRepo = answerRepo;
        _testResultRepo = testResultRepo;
        _seenRepo = seenRepo;
        _resourceRepo = resourceRepo;
        _chapterRepo = chapterRepo;
        _uow = uow;
        _thresholds = thresholds.Value;
    }

    public async Task<TestResultResponse> Handle(
        SubmitTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _testRepo.GetByIdAsync(request.TestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Test), request.TestId);

        if (test.StudentId != request.StudentId)
            throw new UnauthorizedException("This test does not belong to you.");

        if (test.Status == TestStatus.Submitted)
            throw new BadRequestException("This test has already been submitted.");

        bool isExpired = DateTime.UtcNow > test.ExpiresAt;

        // Mark test complete
        test.Status = isExpired ? TestStatus.Expired : TestStatus.Submitted;
        test.SubmittedAt = DateTime.UtcNow;
        test.UpdatedAt = DateTime.UtcNow;
        _testRepo.Update(test);

        // Retrieve answers
        var answers = (await _answerRepo.GetAllAsync(cancellationToken))
            .Where(a => a.TestId == request.TestId)
            .ToList();

        // Algorithm 8 — Test Scoring
        int totalAnswered = answers.Count;
        int correctAnswers = answers.Count(a => a.IsCorrect);
        double score = totalAnswered == 0
            ? 0
            : Math.Round((double)correctAnswers / totalAnswered * 100, 2);

        // Algorithm 9 — Chapter Analysis
        var chapters = await _chapterRepo.GetAllAsync(cancellationToken);
        var chapterMap = chapters.ToDictionary(c => c.Id);

        var chapterGroups = answers
            .GroupBy(a => a.Question.ChapterId)
            .Select(g =>
            {
                var chapter = chapterMap[g.Key];
                var total = g.Count();
                var correct = g.Count(a => a.IsCorrect);
                var pct = total == 0 ? 0 : Math.Round((double)correct / total * 100, 2);

                // Algorithm 10 — Weak Chapter Detection
                bool isWeak = pct < _thresholds.WeakChapterMaxPercent;

                return new ChapterScoreDto(
                    ChapterId: chapter.Id,
                    ChapterName: chapter.Name,
                    StackName: chapter.Stack.Name,
                    ScorePercent: pct,
                    IsWeak: isWeak);
            })
            .ToList();

        // Algorithm 11 — Resource Recommendation
        var weakChapterIds = chapterGroups
            .Where(c => c.IsWeak)
            .Select(c => c.ChapterId)
            .ToHashSet();

        var allResources = await _resourceRepo.GetAllAsync(cancellationToken);
        var recommendations = new List<ResourceRecommendationDto>();

        foreach (var resource in allResources)
        {
            foreach (var mapping in resource.SkillMappings)
            {
                // Recommend if resource is linked to a skill that maps to a weak chapter's stack
                // (Rule-based lookup: Chapter → Stack → Skills mapped to that stack)
                var chapterLinked = chapters
                    .Where(c => weakChapterIds.Contains(c.Id))
                    .Any(c => c.StackId == mapping.Skill.ResourceSkillMappings
                        .Select(m => m.SkillId).Contains(mapping.SkillId) ? c.StackId : -1);

                if (chapterLinked && recommendations.All(r => r.ResourceId != resource.Id))
                {
                    recommendations.Add(new ResourceRecommendationDto(
                        ResourceId: resource.Id,
                        Title: resource.Title,
                        Url: resource.Url,
                        ResourceType: resource.ResourceType,
                        RecommendedBecause: $"Weak chapter coverage"));
                }
            }
        }

        // Bulk-insert seen questions
        var existingSeenIds = (await _seenRepo.GetAllAsync(cancellationToken))
            .Where(s => s.StudentId == request.StudentId)
            .Select(s => s.QuestionId)
            .ToHashSet();

        foreach (var answer in answers.Where(a => !existingSeenIds.Contains(a.QuestionId)))
        {
            await _seenRepo.AddAsync(new StudentSeenQuestion
            {
                StudentId = request.StudentId,
                QuestionId = answer.QuestionId,
                AskedAt = test.StartedAt
            }, cancellationToken);
        }

        // Set all previous TestResults for this student to IsLatest = false
        var previousResults = (await _testResultRepo.GetAllAsync(cancellationToken))
            .Where(tr => tr.StudentId == request.StudentId && tr.IsLatest)
            .ToList();
        foreach (var prev in previousResults)
        {
            prev.IsLatest = false;
            prev.UpdatedAt = DateTime.UtcNow;
            _testResultRepo.Update(prev);
        }

        // Save new TestResult
        var chapterScoresDict = chapterGroups.ToDictionary(c => c.ChapterId.ToString(), c => c.ScorePercent);
        var weakIds = chapterGroups.Where(c => c.IsWeak).Select(c => c.ChapterId).ToList();

        var testResult = new TestResult
        {
            TestId = test.Id,
            StudentId = request.StudentId,
            Score = score,
            TotalAnswered = totalAnswered,
            CorrectAnswers = correctAnswers,
            ChapterScoresJson = System.Text.Json.JsonSerializer.Serialize(chapterScoresDict),
            WeakChapterIdsJson = System.Text.Json.JsonSerializer.Serialize(weakIds),
            IsLatest = true,
            ComputedAt = DateTime.UtcNow
        };
        await _testResultRepo.AddAsync(testResult, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new TestResultResponse(
            TestId: test.Id,
            Score: score,
            TotalAnswered: totalAnswered,
            CorrectAnswers: correctAnswers,
            ChapterScores: chapterGroups,
            WeakChapters: chapterGroups.Where(c => c.IsWeak).Select(c => c.ChapterName).ToList(),
            RecommendedResources: recommendations);
    }
}


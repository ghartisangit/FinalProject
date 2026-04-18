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

// ── Handler ───────────────────────────────────────────────────────────────────

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
    private readonly IRepository<Student> _studentRepo;
    private readonly IUnitOfWork _uow;
    private readonly ThresholdSettings _thresholds;

    public SubmitTestCommandHandler(
        IRepository<Test> testRepo,
        IRepository<TestAnswer> answerRepo,
        IRepository<TestResult> testResultRepo,
        IRepository<StudentSeenQuestion> seenRepo,
        IRepository<Resource> resourceRepo,
        IRepository<Chapter> chapterRepo,
        IRepository<Student> studentRepo,
        IUnitOfWork uow,
        IOptions<ThresholdSettings> thresholds)
    {
        _testRepo = testRepo;
        _answerRepo = answerRepo;
        _testResultRepo = testResultRepo;
        _seenRepo = seenRepo;
        _resourceRepo = resourceRepo;
        _chapterRepo = chapterRepo;
        _studentRepo = studentRepo;
        _uow = uow;
        _thresholds = thresholds.Value;
    }

    public async Task<TestResultResponse> Handle(
        SubmitTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _testRepo.GetByIdAsync(request.TestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Test), request.TestId);

        // Resolve UserId → StudentId
        var studentAll = await _studentRepo.GetAllAsync(cancellationToken);
        var student = studentAll.FirstOrDefault(s => s.UserId == request.UserId)
            ?? throw new NotFoundException("No student profile found for this user.");
        var studentId = student.Id;

        if (test.StudentId != studentId)
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

        // Algorithm 11 — Resource Recommendation (rule-based lookup)
        // Weak chapters → find skills mapped to those chapters' stacks → find resources for those skills
        var weakChapterIds = chapterGroups
            .Where(c => c.IsWeak)
            .Select(c => c.ChapterId)
            .ToHashSet();

        var allResources = await _resourceRepo.GetAllAsync(cancellationToken);
        var recommendations = new List<ResourceRecommendationDto>();
        var addedResourceIds = new HashSet<int>();

        // Path A: Weak chapter → Chapter name used to recommend resources via ResourceSkillMapping
        // Each resource is linked to Skills; chapters share a Stack; we match by chapter name tag
        var weakChapterNames = chapterGroups
            .Where(c => c.IsWeak)
            .ToDictionary(c => c.ChapterId, c => c.ChapterName);

        foreach (var resource in allResources)
        {
            if (addedResourceIds.Contains(resource.Id)) continue;

            // Check if any skill mapped to this resource belongs to a weak chapter's stack
            foreach (var skillMapping in resource.SkillMappings)
            {
                // Find chapters whose stack contains this skill (via VacancySkills or StudentSkills)
                // Simple rule: if resource title/description mentions a weak chapter name → recommend
                var matchedChapter = weakChapterNames.Values
                    .FirstOrDefault(name =>
                        resource.Title.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        (resource.Description ?? "").Contains(name, StringComparison.OrdinalIgnoreCase));

                if (matchedChapter is not null)
                {
                    recommendations.Add(new ResourceRecommendationDto(
                        ResourceId: resource.Id,
                        Title: resource.Title,
                        Url: resource.Url,
                        ResourceType: resource.ResourceType,
                        RecommendedBecause: $"Weak chapter: {matchedChapter}"));
                    addedResourceIds.Add(resource.Id);
                    break;
                }
            }
        }

        // Path B: MissingSkills (from student profile gaps) → ResourceSkillMapping → resources
        var studentSkillIds = student.StudentSkills.Select(ss => ss.SkillId).ToHashSet();
        foreach (var resource in allResources.Where(r => !addedResourceIds.Contains(r.Id)))
        {
            foreach (var mapping in resource.SkillMappings)
            {
                if (!studentSkillIds.Contains(mapping.SkillId))
                {
                    recommendations.Add(new ResourceRecommendationDto(
                        ResourceId: resource.Id,
                        Title: resource.Title,
                        Url: resource.Url,
                        ResourceType: resource.ResourceType,
                        RecommendedBecause: $"Missing skill: {mapping.Skill.Name}"));
                    addedResourceIds.Add(resource.Id);
                    break;
                }
            }
        }

        // Bulk-insert seen questions
        var existingSeenIds = (await _seenRepo.GetAllAsync(cancellationToken))
            .Where(s => s.StudentId == studentId)
            .Select(s => s.QuestionId)
            .ToHashSet();

        foreach (var answer in answers.Where(a => !existingSeenIds.Contains(a.QuestionId)))
        {
            await _seenRepo.AddAsync(new StudentSeenQuestion
            {
                StudentId = studentId,
                QuestionId = answer.QuestionId,
                AskedAt = test.StartedAt
            }, cancellationToken);
        }

        // Set all previous TestResults for this student to IsLatest = false
        var previousResults = (await _testResultRepo.GetAllAsync(cancellationToken))
            .Where(tr => tr.StudentId == studentId && tr.IsLatest)
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
            StudentId = studentId,
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

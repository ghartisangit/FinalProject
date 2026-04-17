using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Ranking;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Applications.Queries.GetRankedCandidates;

public class GetRankedCandidatesQueryHandler
    : IRequestHandler<GetRankedCandidatesQuery, RankedCandidateListResponse>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;
    private readonly IRepository<TestResult> _testResultRepo;
    private readonly IScoringService _scoring;

    public GetRankedCandidatesQueryHandler(
        IRepository<Vacancy> vacancyRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo,
        IRepository<TestResult> testResultRepo,
        IScoringService scoring)
    {
        _vacancyRepo = vacancyRepo;
        _applicationRepo = applicationRepo;
        _testResultRepo = testResultRepo;
        _scoring = scoring;
    }

    public async Task<RankedCandidateListResponse> Handle(
        GetRankedCandidatesQuery request, CancellationToken cancellationToken)
    {
        var vacancy = await _vacancyRepo.GetByIdAsync(request.VacancyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        if (vacancy.OrganizationId != request.OrganizationId)
            throw new UnauthorizedException("You do not own this vacancy.");

        var applications = (await _applicationRepo.GetAllAsync(cancellationToken))
            .Where(a => a.VacancyId == request.VacancyId && a.MatchSnapshot != null)
            .ToList();

        var allTestResults = await _testResultRepo.GetAllAsync(cancellationToken);
        var latestScores = allTestResults
            .Where(tr => tr.IsLatest)
            .ToDictionary(tr => tr.StudentId, tr => tr.Score);

        // Eligibility filter
        var eligible = applications
            .Where(a => a.MatchSnapshot!.RequirementFit >= 60)
            .ToList();

        // Score and rank
        var ranked = eligible
            .Select(a =>
            {
                var latestTestScore = latestScores.GetValueOrDefault(a.StudentId, 0);
                return _scoring.Score(a.Student, a.MatchSnapshot!, latestTestScore, a.Status.ToString(), rank: 0);
            })
            .OrderByDescending(c => c.FinalScore)
            .Select((c, i) => c with { Rank = i + 1 })
            .ToList();

        return new RankedCandidateListResponse(
            VacancyId: vacancy.Id,
            VacancyTitle: vacancy.Title,
            Candidates: ranked);
    }
}
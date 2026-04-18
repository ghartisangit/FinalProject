using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetOrganizationVacancies;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns all vacancies (published and draft) belonging to the requesting organization.
/// </summary>
public record GetOrganizationVacanciesQuery(int OrganizationId)
    : IRequest<IReadOnlyList<VacancyResponse>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetOrganizationVacanciesQueryHandler
    : IRequestHandler<GetOrganizationVacanciesQuery, IReadOnlyList<VacancyResponse>>
{
    private readonly IRepository<Vacancy> _vacancyRepo;

    public GetOrganizationVacanciesQueryHandler(IRepository<Vacancy> vacancyRepo)
        => _vacancyRepo = vacancyRepo;

    public async Task<IReadOnlyList<VacancyResponse>> Handle(
        GetOrganizationVacanciesQuery request, CancellationToken cancellationToken)
    {
        var all = await _vacancyRepo.GetAllAsync(cancellationToken);

        return all
            .Where(v => v.OrganizationId == request.OrganizationId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VacancyResponse(
                VacancyId: v.Id,
                OrganizationId: v.OrganizationId,
                OrganizationName: v.Organization.Name,
                Title: v.Title,
                Description: v.Description,
                IsPublished: v.IsPublished,
                PublishedAt: v.PublishedAt,
                RequiredEducationLevel: v.RequiredEducationLevel?.ToString(),
                RequiredFieldOfStudy: v.RequiredFieldOfStudy,
                RequiredSkills: v.VacancySkills
                    .Where(vs => vs.IsRequired)
                    .Select(vs => new VacancySkillDto(vs.SkillId, vs.Skill.Name))
                    .ToList(),
                OptionalSkills: v.VacancySkills
                    .Where(vs => !vs.IsRequired)
                    .Select(vs => new VacancySkillDto(vs.SkillId, vs.Skill.Name))
                    .ToList()))
            .ToList();
    }
}

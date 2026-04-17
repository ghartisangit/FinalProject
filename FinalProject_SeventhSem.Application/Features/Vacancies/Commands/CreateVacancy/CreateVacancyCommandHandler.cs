using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.CreateVacancy;

public class CreateVacancyCommandHandler : IRequestHandler<CreateVacancyCommand, VacancyResponse>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<VacancySkill> _vacancySkillRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IUnitOfWork _uow;

    public CreateVacancyCommandHandler(
        IRepository<Vacancy> vacancyRepo,
        IRepository<VacancySkill> vacancySkillRepo,
        IRepository<Skill> skillRepo,
        IUnitOfWork uow)
    {
        _vacancyRepo = vacancyRepo;
        _vacancySkillRepo = vacancySkillRepo;
        _skillRepo = skillRepo;
        _uow = uow;
    }

    public async Task<VacancyResponse> Handle(
        CreateVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancy = new Vacancy
        {
            OrganizationId = request.OrganizationId,
            Title = request.Title,
            Description = request.Description,
            RequiredEducationLevel = request.RequiredEducationLevel,
            RequiredFieldOfStudy = request.RequiredFieldOfStudy,
            IsPublished = false
        };

        await _vacancyRepo.AddAsync(vacancy, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Add required skills
        foreach (var skillId in request.RequiredSkillIds.Distinct())
            await _vacancySkillRepo.AddAsync(
                new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = true },
                cancellationToken);

        // Add optional skills (skip any already in required)
        foreach (var skillId in request.OptionalSkillIds.Distinct()
                     .Where(id => !request.RequiredSkillIds.Contains(id)))
            await _vacancySkillRepo.AddAsync(
                new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = false },
                cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        // Build response
        var allSkills = await _skillRepo.GetAllAsync(cancellationToken);
        var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);

        return new VacancyResponse(
            VacancyId: vacancy.Id,
            OrganizationId: vacancy.OrganizationId,
            OrganizationName: string.Empty, // populated by query layer
            Title: vacancy.Title,
            Description: vacancy.Description,
            IsPublished: vacancy.IsPublished,
            PublishedAt: vacancy.PublishedAt,
            RequiredEducationLevel: vacancy.RequiredEducationLevel?.ToString(),
            RequiredFieldOfStudy: vacancy.RequiredFieldOfStudy,
            RequiredSkills: request.RequiredSkillIds
                                        .Select(id => new VacancySkillDto(id, skillMap.GetValueOrDefault(id, "Unknown")))
                                        .ToList(),
            OptionalSkills: request.OptionalSkillIds
                                        .Select(id => new VacancySkillDto(id, skillMap.GetValueOrDefault(id, "Unknown")))
                                        .ToList());
    }
}


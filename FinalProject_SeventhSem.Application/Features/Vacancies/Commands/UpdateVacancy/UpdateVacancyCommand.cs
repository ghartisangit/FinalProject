using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.UpdateVacancy;

public record UpdateVacancyCommand(
    int VacancyId,
    int OrganizationId,
    string Title,
    string Description,
    EducationLevel? RequiredEducationLevel,
    string? RequiredFieldOfStudy,
    IReadOnlyList<int> RequiredSkillIds,
    IReadOnlyList<int> OptionalSkillIds
) : IRequest<VacancyResponse>;

// ── Validator ─────────────────────────────────────────────────────────────────

public class UpdateVacancyCommandValidator : AbstractValidator<UpdateVacancyCommand>
{
    public UpdateVacancyCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.RequiredSkillIds)
            .NotEmpty()
            .WithMessage("At least one required skill must be specified.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate required skill IDs are not allowed.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class UpdateVacancyCommandHandler : IRequestHandler<UpdateVacancyCommand, VacancyResponse>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<VacancySkill> _vacancySkillRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IUnitOfWork _uow;

    public UpdateVacancyCommandHandler(
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
        UpdateVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancy = await _vacancyRepo.GetByIdAsync(request.VacancyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        if (vacancy.OrganizationId != request.OrganizationId)
            throw new UnauthorizedException("You do not own this vacancy.");

        if (vacancy.IsPublished)
            throw new BadRequestException("Cannot edit a published vacancy. Unpublish it first.");

        // Update scalar fields
        vacancy.Title = request.Title;
        vacancy.Description = request.Description;
        vacancy.RequiredEducationLevel = request.RequiredEducationLevel;
        vacancy.RequiredFieldOfStudy = request.RequiredFieldOfStudy;
        vacancy.UpdatedAt = DateTime.UtcNow;
        _vacancyRepo.Update(vacancy);

        // Replace skills: remove all existing, re-add from request
        var existingSkills = vacancy.VacancySkills.ToList();
        foreach (var vs in existingSkills)
            _vacancySkillRepo.Remove(vs);

        await _uow.SaveChangesAsync(cancellationToken);

        foreach (var skillId in request.RequiredSkillIds.Distinct())
            await _vacancySkillRepo.AddAsync(
                new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = true },
                cancellationToken);

        foreach (var skillId in request.OptionalSkillIds.Distinct()
                     .Where(id => !request.RequiredSkillIds.Contains(id)))
            await _vacancySkillRepo.AddAsync(
                new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = false },
                cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        var allSkills = await _skillRepo.GetAllAsync(cancellationToken);
        var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);

        return new VacancyResponse(
            VacancyId: vacancy.Id,
            OrganizationId: vacancy.OrganizationId,
            OrganizationName: vacancy.Organization.Name,
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

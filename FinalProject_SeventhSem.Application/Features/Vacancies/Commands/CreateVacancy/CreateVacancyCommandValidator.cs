using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.CreateVacancy;

public class CreateVacancyCommandValidator : AbstractValidator<CreateVacancyCommand>
{
    public CreateVacancyCommandValidator()
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



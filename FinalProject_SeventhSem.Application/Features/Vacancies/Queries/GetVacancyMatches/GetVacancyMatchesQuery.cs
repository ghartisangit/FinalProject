using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancyMatches;

/// <summary>
/// Returns all published vacancies with match scores for the requesting student.
/// Runs Algorithms 3–6 per vacancy. Sorted: RequirementFit → OptionalFit → EducationBonus DESC.
/// Ineligible vacancies are included with IsEligible = false and an EligibilityMessage.
/// </summary>
public record GetVacancyMatchesQuery(int StudentId) : IRequest<IReadOnlyList<VacancyMatchResult>>;

// ── Handler ───────────────────────────────────────────────────────────────────


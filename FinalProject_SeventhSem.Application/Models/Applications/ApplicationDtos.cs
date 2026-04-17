using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Applications;

public record ApplyToVacancyRequest(
    int VacancyId
);

public record UpdateApplicationStatusRequest(
    ApplicationStatus NewStatus
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record ApplicationResponse(
    int ApplicationId,
    int VacancyId,
    string VacancyTitle,
    int StudentId,
    string StudentName,
    string Status,
    DateTime AppliedAt,
    DateTime? StatusUpdatedAt,
    ApplicationSnapshotDto? MatchSnapshot
);

/// <summary>
/// Flattened view of the ApplicationMatchSnapshot stored at apply-time.
/// </summary>
public record ApplicationSnapshotDto(
    double RequirementFit,
    double OptionalFit,
    double EducationBonus,
    IReadOnlyList<string> MissingSkills,
    DateTime ComputedAt
);


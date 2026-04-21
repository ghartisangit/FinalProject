using FinalProject_SeventhSem.Domain.Common;
using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Vacancy : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }

    // Education requirements (optional at entity level; rules enforced in application layer)
    public EducationLevel? RequiredEducationLevel { get; set; }
    public string? RequiredFieldOfStudy { get; set; }

    /// <summary>
    /// Optional deadline for applications.
    /// If set, students cannot apply after this date/time (UTC).
    /// If null, the vacancy accepts applications indefinitely.
    /// </summary>
    public DateTime ApplicationDeadline { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<VacancySkill> VacancySkills { get; set; } = new List<VacancySkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}

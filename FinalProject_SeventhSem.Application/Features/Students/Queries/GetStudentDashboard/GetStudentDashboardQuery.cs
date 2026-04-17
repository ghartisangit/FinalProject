using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Students;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentDashboard;

public record GetStudentDashboardQuery(int StudentId) : IRequest<ProfileCompletenessResponse>;

// ── Handler ───────────────────────────────────────────────────────────────────

/// <summary>
/// Computes profile completeness on the fly (Option A).
/// Max score = 100. No extra DB column required.
///
/// Scoring table:
///   FullName       5  |  Photo         5  |  Phone       5
///   Education     15  |  Skills     0-20  |  Resume     15
///   GitHub        10  |  Portfolio  10    |  LinkedIn    5
///   Bio            5  |  Nationality  5
/// </summary>

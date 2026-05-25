using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAdminDashboard;


public record AdminDashboardSummaryResponse(
    int TotalStudents,
    int VerifiedOrganizations,
    int TotalVacancies,
    int TotalApplications
);
public record GetAdminDashboardSummaryQuery : IRequest<AdminDashboardSummaryResponse>;

public class GetAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetAdminDashboardSummaryQuery, AdminDashboardSummaryResponse>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Organization> _organizationRepo;
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;

    public GetAdminDashboardSummaryQueryHandler(
        IRepository<Student> studentRepo,
        IRepository<Organization> organizationRepo,
        IRepository<Vacancy> vacancyRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo)
    {
        _studentRepo = studentRepo;
        _organizationRepo = organizationRepo;
        _vacancyRepo = vacancyRepo;
        _applicationRepo = applicationRepo;
    }

    public async Task<AdminDashboardSummaryResponse> Handle(
    GetAdminDashboardSummaryQuery request,
    CancellationToken cancellationToken)
    {
        // Await each query individually so they execute one after the other 
        // using the DbContext safely.
        var studentCount = await _studentRepo.CountAsync(cancellationToken);

        var verifiedOrgCount = await _organizationRepo.CountAsync(
            predicate: o => o.Status == OrganizationStatus.Verified,
            cancellationToken: cancellationToken);

        var vacancyCount = await _vacancyRepo.CountAsync(cancellationToken);

        var applicationCount = await _applicationRepo.CountAsync(cancellationToken);

        // Return the summary object directly
        return new AdminDashboardSummaryResponse(
            TotalStudents: studentCount,
            VerifiedOrganizations: verifiedOrgCount,
            TotalVacancies: vacancyCount,
            TotalApplications: applicationCount
        );
    }
}

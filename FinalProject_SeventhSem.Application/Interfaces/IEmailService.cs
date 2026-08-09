using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface IEmailService
{
    Task SendRegistrationSuccessEmailAsync(string toEmail, string fullName, CancellationToken ct = default);
    Task SendOrganizationPendingApprovalEmailAsync(string toEmail, string organizationName, CancellationToken ct = default);
    Task SendOrganizationApprovedEmailAsync(string toEmail, string organizationName, CancellationToken ct = default);
    Task SendOrganizationRejectedEmailAsync(string toEmail, string organizationName, string? reason, CancellationToken ct = default);
}
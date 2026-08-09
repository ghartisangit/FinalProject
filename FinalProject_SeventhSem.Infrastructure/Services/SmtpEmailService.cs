using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly IHostEnvironment _env;

    public SmtpEmailService(
        IOptions<EmailSettings> settings,
        ILogger<SmtpEmailService> logger,
        IHostEnvironment env)
    {
        _settings = settings.Value;
        _logger = logger;
        _env = env;
    }

    public Task SendRegistrationSuccessEmailAsync(string toEmail, string fullName, CancellationToken ct = default)
    {
        var subject = "Welcome to InternHub — Registration Successful";
        var body = $@"
            <div style='font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:auto'>
              <h2 style='color:#2c3e50'>Welcome to InternHub, {fullName}!</h2>
              <p>Your account has been registered successfully.</p>
              <p>You can now log in and start exploring internship opportunities matched to your profile.</p>
              <p style='margin-top:24px'>
                <a href='{_settings.AppBaseUrl}/login'
                   style='background:#2c3e50;color:#fff;padding:10px 18px;text-decoration:none;border-radius:4px'>
                  Go to InternHub
                </a>
              </p>
            </div>";

        return SendAsync(toEmail, subject, body, ct);
    }

    public Task SendOrganizationPendingApprovalEmailAsync(string toEmail, string organizationName, CancellationToken ct = default)
    {
        var subject = "InternHub — Registration Received, Pending Approval";
        var body = $@"
            <div style='font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:auto'>
              <h2 style='color:#2c3e50'>Thanks for registering, {organizationName}!</h2>
              <p>Your organization account has been created successfully and is currently <strong>pending admin approval</strong>.</p>
              <p>You'll receive another email as soon as your account has been reviewed. You won't be able to log in until it's approved.</p>
            </div>";

        return SendAsync(toEmail, subject, body, ct);
    }

    public Task SendOrganizationApprovedEmailAsync(string toEmail, string organizationName, CancellationToken ct = default)
    {
        var subject = "Congratulations! Your InternHub Organization Account is Approved";
        var body = $@"
            <div style='font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:auto'>
              <h2 style='color:#2c7a39'>Congratulations, {organizationName}!</h2>
              <p>Your organization account has been <strong>approved</strong> by our admin team.</p>
              <p>You can now log in and start posting internship opportunities.</p>
              <p style='margin-top:24px'>
                <a href='{_settings.AppBaseUrl}/login'
                   style='background:#2c7a39;color:#fff;padding:10px 18px;text-decoration:none;border-radius:4px'>
                  Log in to InternHub
                </a>
              </p>
            </div>";

        return SendAsync(toEmail, subject, body, ct);
    }

    public Task SendOrganizationRejectedEmailAsync(string toEmail, string organizationName, string? reason, CancellationToken ct = default)
    {
        var subject = "InternHub — Organization Application Update";
        var reasonBlock = string.IsNullOrWhiteSpace(reason)
            ? ""
            : $"<p><strong>Reason:</strong> {reason}</p>";

        var body = $@"
            <div style='font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:auto'>
              <h2 style='color:#b02a2a'>Application Update</h2>
              <p>Dear {organizationName},</p>
              <p>We regret to inform you that your organization application on InternHub was <strong>not approved</strong> at this time.</p>
              {reasonBlock}
              <p>If you believe this was a mistake or would like to reapply with updated details, please contact support.</p>
            </div>";

        return SendAsync(toEmail, subject, body, ct);
    }

    private async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        if (_env.IsDevelopment() && _settings.UseConsoleInDevelopment)
        {
            _logger.LogInformation("[DEV EMAIL] To: {To}\nSubject: {Subject}\n{Body}", toEmail, subject, body);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword)
        };

        try
        {
            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
        }
    }
}
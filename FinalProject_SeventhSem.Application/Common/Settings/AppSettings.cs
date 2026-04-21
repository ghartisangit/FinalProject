using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Common.Settings;

public class TestSettings
{
    public const string SectionName = "TestSettings";
    public int TestDurationMinutes { get; set; } = 30;
    public int QuestionsPerTest { get; set; } = 20;
}

/// <summary>Maps to "Thresholds" in appsettings.json.</summary>
public class ThresholdSettings
{
    public const string SectionName = "Thresholds";
    public double EligibilityMinPercent { get; set; } = 60;
    public double WeakChapterMaxPercent { get; set; } = 50;
    public double AptitudeBonusWeight { get; set; } = 0.30;
    public double OptionalSkillWeight { get; set; } = 0.15;
    public double GitHubBonus { get; set; } = 4;
    public double PortfolioBonus { get; set; } = 4;
    public double LinkedInBonus { get; set; } = 2;
    public double EducationOptionalBonus { get; set; } = 5;

    /// <summary>
    /// Max possible RankingScore — denominator for FinalScore normalisation.
    /// RequirementFit(100) + TestScore*0.30(30) + OptionalFit*0.15(15)
    /// + EducationBonus(5) + ProfileBonus(10) = 160.
    /// Documented here so changing weights stays in sync.
    /// </summary>
    public double MaxRankingScore => 160;
}

/// <summary>Maps to "ResumeParsingSettings" in appsettings.json.</summary>
public class ResumeParsingSettings
{
    public const string SectionName = "ResumeParsingSettings";
    public int MinTokenLength { get; set; } = 2;
    public int MaxSuggestionsCount { get; set; } = 30;
    public List<string> StopWords { get; set; } = [];
}

/// <summary>Maps to "JwtSettings" in appsettings.json.</summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

/// <summary>Maps to "AdminSeed" in appsettings.json. Used by DatabaseSeeder on startup.</summary>
public class AdminSeedSettings
{
    public const string SectionName = "AdminSeed";
    public string Email { get; set; } = "admin@internhub.com";
    public string Password { get; set; } = "Admin@1234";
    public string FullName { get; set; } = "System Administrator";
}

/// <summary>
/// Maps to "EmailSettings" in appsettings.json.
/// Used by EmailService (Infrastructure) via IEmailService.
/// SmtpHost/Port/User/Password are loaded from environment variables in production.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    /// <summary>SMTP server hostname. e.g. smtp.gmail.com</summary>
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    /// <summary>SMTP port. 587 for TLS (STARTTLS), 465 for SSL.</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>Enable STARTTLS. Set true for port 587.</summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>SMTP login username (usually the sender email address).</summary>
    public string SmtpUser { get; set; } = string.Empty;

    /// <summary>SMTP login password or app password.</summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>The From address shown to recipients.</summary>
    public string FromEmail { get; set; } = "noreply@internhub.com";

    /// <summary>The From display name shown to recipients.</summary>
    public string FromName { get; set; } = "InternHub";

    /// <summary>
    /// Base URL of the frontend app. Used to build links inside emails
    /// e.g. "https://internhub.com" → login link becomes "https://internhub.com/login"
    /// </summary>
    public string AppBaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// When true the email body is written to the console instead of sent via SMTP.
    /// Useful for development where no real SMTP server is configured.
    /// </summary>
    public bool UseConsoleInDevelopment { get; set; } = true;
}

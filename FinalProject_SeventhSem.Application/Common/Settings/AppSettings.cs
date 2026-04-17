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

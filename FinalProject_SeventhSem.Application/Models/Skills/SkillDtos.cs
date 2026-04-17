using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Skills;

public record CreateSkillRequest(
    string Name
);

public record UpdateSkillRequest(
    string Name
);

/// <summary>
/// Admin creates/updates an alias for an existing skill.
/// Guards (enforced by FluentValidation):
///   - Alias.Length >= 2
///   - Alias is not a stopword
///   - Alias is not already mapped to a different SkillId
/// </summary>
public record CreateSkillAliasRequest(
    int SkillId,
    string Alias
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record SkillResponse(
    int SkillId,
    string Name,
    IReadOnlyList<string> Aliases
);

public record SkillAliasResponse(
    int AliasId,
    int SkillId,
    string SkillName,
    string Alias
);

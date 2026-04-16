using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class SkillAlias : BaseEntity
{
    public int SkillId { get; set; }

    /// <summary>Lowercase alias stored for case-insensitive matching in Algorithm 2.</summary>
    public string Alias { get; set; } = string.Empty;

    // Navigation
    public Skill Skill { get; set; } = null!;
}

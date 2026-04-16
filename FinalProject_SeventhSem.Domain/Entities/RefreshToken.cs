using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }

    /// <summary>BCrypt hash of the raw refresh token sent to the client.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>FK to the RefreshToken that replaced this one during rotation.</summary>
    public int? ReplacedByTokenId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public RefreshToken? ReplacedByToken { get; set; }
}


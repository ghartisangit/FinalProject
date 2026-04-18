using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;




/// <summary>
/// Stores BCrypt-hashed refresh tokens for JWT rotation.
/// Rotation flow:
///   1. On login  → generate random 64-byte token, BCrypt-hash it, store here.
///   2. On refresh → hash incoming token, match against TokenHash.
///   3. If valid   → issue new pair; set IsRevoked = true, populate ReplacedByTokenId.
///   4. If already revoked → revoke entire chain (security breach response).
/// </summary>
public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }

    /// <summary>BCrypt hash of the raw refresh token sent to the client.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Non-sensitive lookup key stored in plain text.
    /// Generated as the first 16 chars of the Base64 raw token.
    /// Used to fetch the single matching DB row before BCrypt.Verify,
    /// avoiding the O(n) full-table scan over BCrypt hashes.
    /// </summary>
    public string TokenLookup { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>FK to the RefreshToken that replaced this one during rotation.</summary>
    public int? ReplacedByTokenId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public RefreshToken? ReplacedByToken { get; set; }
}

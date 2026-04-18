using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

/// <summary>
/// Contract for refresh token generation, hashing, and rotation.
/// Keeps BCrypt dependency out of the Application layer.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a cryptographically random 64-byte raw token string.</summary>
    string GenerateRawRefreshToken();

    /// <summary>
    /// Derives a non-sensitive plain-text lookup key from the raw token.
    /// Stored in DB so we can fetch a single row before BCrypt.Verify,
    /// avoiding a full-table O(n) BCrypt scan.
    /// </summary>
    string GenerateLookupKey(string rawToken);

    /// <summary>BCrypt-hashes the raw token for storage.</summary>
    string HashToken(string rawToken);

    /// <summary>Verifies a raw token against its stored BCrypt hash.</summary>
    bool VerifyToken(string rawToken, string hash);
}

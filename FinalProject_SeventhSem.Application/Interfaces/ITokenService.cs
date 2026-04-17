using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface ITokenService
{
    /// <summary>Generates a cryptographically random 64-byte raw token string.</summary>
    string GenerateRawRefreshToken();

    /// <summary>BCrypt-hashes the raw token for storage.</summary>
    string HashToken(string rawToken);

    /// <summary>Verifies a raw token against its stored BCrypt hash.</summary>
    bool VerifyToken(string rawToken, string hash);
}

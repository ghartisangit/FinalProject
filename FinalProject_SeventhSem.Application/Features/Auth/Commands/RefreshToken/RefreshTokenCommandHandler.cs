using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Auth;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> _refreshTokenRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> refreshTokenRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow,
        ITokenService tokenService,
        IJwtService jwtService)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _userRepo = userRepo;
        _uow = uow;
        _tokenService = tokenService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find matching token by verifying BCrypt hash
        var allTokens = await _refreshTokenRepo.GetAllAsync(cancellationToken);
        var stored = allTokens.FirstOrDefault(t =>
            !t.IsRevoked &&
            t.ExpiresAt > DateTime.UtcNow &&
            _tokenService.VerifyToken(request.RawRefreshToken, t.TokenHash));

        if (stored is null)
        {
            // Check if the token is revoked (breach detection)
            var revoked = allTokens.FirstOrDefault(t =>
                t.IsRevoked && _tokenService.VerifyToken(request.RawRefreshToken, t.TokenHash));

            if (revoked is not null)
            {
                // Revoke entire chain
                var chain = allTokens.Where(t => t.UserId == revoked.UserId).ToList();
                foreach (var t in chain) t.IsRevoked = true;
                await _uow.SaveChangesAsync(cancellationToken);
            }

            throw new InvalidRefreshTokenException();
        }

        var user = await _userRepo.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), stored.UserId);

        // Rotate: revoke old, issue new
        stored.IsRevoked = true;

        var rawNew = _tokenService.GenerateRawRefreshToken();
        var newToken = new FinalProject_SeventhSem.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(rawNew),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepo.AddAsync(newToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        stored.ReplacedByTokenId = newToken.Id;
        await _uow.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: _jwtService.GenerateAccessToken(user),
            RefreshToken: rawNew,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15),
            User: new UserSummary(user.Id, user.Email, user.Role.ToString()));
    }
}


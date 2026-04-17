using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.VerifyOrganization;

public class VerifyOrganizationCommandHandler : IRequestHandler<VerifyOrganizationCommand>
{
    private readonly IRepository<Organization> _orgRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public VerifyOrganizationCommandHandler(
        IRepository<Organization> orgRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow)
    {
        _orgRepo = orgRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task Handle(VerifyOrganizationCommand request, CancellationToken cancellationToken)
    {
        var org = await _orgRepo.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        if (org.IsVerified)
            throw new BadRequestException("Organization is already verified.");

        org.IsVerified = true;
        org.UpdatedAt = DateTime.UtcNow;
        _orgRepo.Update(org);

        // Activate the linked user account so they can log in
        var user = await _userRepo.GetByIdAsync(org.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), org.UserId);

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);

        await _uow.SaveChangesAsync(cancellationToken);
    }
}


using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Applications.Commands.UpdateApplicationStatus;

public class UpdateApplicationStatusCommandHandler
    : IRequestHandler<UpdateApplicationStatusCommand>
{
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;
    private readonly IUnitOfWork _uow;

    public UpdateApplicationStatusCommandHandler(
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo, IUnitOfWork uow)
    {
        _applicationRepo = applicationRepo;
        _uow = uow;
    }

    public async Task Handle(
        UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepo.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Application), request.ApplicationId);

        // Ensure the vacancy belongs to this organisation
        if (application.Vacancy.OrganizationId != request.OrganizationId)
            throw new UnauthorizedException("You do not own the vacancy for this application.");

        application.Status = request.NewStatus;
        application.StatusUpdatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        _applicationRepo.Update(application);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}



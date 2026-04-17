using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.VerifyOrganization;

public record VerifyOrganizationCommand(int OrganizationId) : IRequest;

public class VerifyOrganizationCommandValidator : AbstractValidator<VerifyOrganizationCommand>
{
    public VerifyOrganizationCommandValidator()
        => RuleFor(x => x.OrganizationId).GreaterThan(0);
}
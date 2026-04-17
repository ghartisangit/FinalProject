using FinalProject_SeventhSem.Application.Models.Skills;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageSkill;

public record CreateSkillCommand(string Name) : IRequest<SkillResponse>;
public record CreateSkillAliasCommand(int SkillId, string Alias) : IRequest<SkillAliasResponse>;

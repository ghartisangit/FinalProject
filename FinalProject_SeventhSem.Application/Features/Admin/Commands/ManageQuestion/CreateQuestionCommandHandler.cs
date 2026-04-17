using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Stacks;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageQuestion;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, QuestionResponse>
{
    private readonly IRepository<Question> _questionRepo;
    private readonly IRepository<Chapter> _chapterRepo;
    private readonly IUnitOfWork _uow;

    public CreateQuestionCommandHandler(
        IRepository<Question> questionRepo,
        IRepository<Chapter> chapterRepo,
        IUnitOfWork uow)
    {
        _questionRepo = questionRepo;
        _chapterRepo = chapterRepo;
        _uow = uow;
    }

    public async Task<QuestionResponse> Handle(
        CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepo.GetByIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(nameof(Chapter), request.ChapterId);

        var question = new Question
        {
            ChapterId = chapter.Id,
            Text = request.Text,
            OptionA = request.OptionA,
            OptionB = request.OptionB,
            OptionC = request.OptionC,
            OptionD = request.OptionD,
            CorrectOption = request.CorrectOption.ToUpper()
        };
        await _questionRepo.AddAsync(question, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new QuestionResponse(
            QuestionId: question.Id,
            ChapterId: chapter.Id,
            ChapterName: chapter.Name,
            StackName: chapter.Stack.Name,
            Text: question.Text,
            OptionA: question.OptionA,
            OptionB: question.OptionB,
            OptionC: question.OptionC,
            OptionD: question.OptionD,
            CorrectOption: question.CorrectOption);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// DELETE QUESTION
// ════════════════════════════════════════════════════════════════════════════

public record DeleteQuestionCommand(int QuestionId) : IRequest;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IRepository<Question> _questionRepo;
    private readonly IUnitOfWork _uow;

    public DeleteQuestionCommandHandler(IRepository<Question> questionRepo, IUnitOfWork uow)
    {
        _questionRepo = questionRepo;
        _uow = uow;
    }

    public async Task Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _questionRepo.GetByIdAsync(request.QuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        _questionRepo.Remove(question);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}


using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Resume;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Resume.Commands.UploadResume;

public class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, ResumeParseResponse>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IResumeParsingService _parser;
    private readonly IFileStorageService _storage;

    public UploadResumeCommandHandler(
        IRepository<Student> studentRepo,
        IUnitOfWork uow,
        IResumeParsingService parser,
        IFileStorageService storage)
    {
        _studentRepo = studentRepo;
        _uow = uow;
        _parser = parser;
        _storage = storage;
    }

    public async Task<ResumeParseResponse> Handle(
        UploadResumeCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepo.GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Student), request.StudentId);

        // Delete old resume if exists
        if (!string.IsNullOrWhiteSpace(student.ResumeUrl))
            await _storage.DeleteAsync(student.ResumeUrl, cancellationToken);

        // Save PDF
        var url = await _storage.SaveAsync(request.PdfStream, request.FileName, "resumes", cancellationToken);
        student.ResumeUrl = url;
        student.UpdatedAt = DateTime.UtcNow;
        _studentRepo.Update(student);
        await _uow.SaveChangesAsync(cancellationToken);

        // Algorithm 1 → Algorithm 2
        var rawText = await _parser.ExtractTextAsync(request.PdfStream);
        var cleanText = _parser.PreprocessText(rawText);
        return await _parser.ExtractSkillsAsync(cleanText);
    }
}


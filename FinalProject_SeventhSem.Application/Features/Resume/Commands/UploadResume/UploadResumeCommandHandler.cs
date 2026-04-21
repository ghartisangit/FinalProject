using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Resume;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Resume.Commands.UploadResume;

public class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, ResumeParseResponse>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<StudentSkill> _studentSkillRepo;
    private readonly IUnitOfWork _uow;
    private readonly IResumeParsingService _parser;
    private readonly IFileStorageService _storage;

    public UploadResumeCommandHandler(
        IRepository<Student> studentRepo,
         IRepository<StudentSkill> studentSkillRepo,
        IUnitOfWork uow,
        IResumeParsingService parser,
        IFileStorageService storage)
    {
        _studentRepo = studentRepo;
        _studentSkillRepo = studentSkillRepo;
        _uow = uow;
        _parser = parser;
        _storage = storage;
    }

    public async Task<ResumeParseResponse> Handle(
        UploadResumeCommand request, CancellationToken cancellationToken)
    {
        //var student = await StudentResolver.ResolveAsync(request.UserId, _studentRepo, cancellationToken);
        try
        {

            var student = await _studentRepo.GetByIdAsync(
            request.UserId,
            q => q.Include(s => s.StudentSkills),
            cancellationToken)
            ?? throw new NotFoundException("Student not found.");

            // Delete old resume if exists
            if (!string.IsNullOrWhiteSpace(student.ResumeUrl))
                await _storage.DeleteAsync(student.ResumeUrl, cancellationToken);

            using var storageStream = new MemoryStream();
            using var parseStream = new MemoryStream();

            await request.PdfStream.CopyToAsync(storageStream, cancellationToken);
            storageStream.Position = 0;
            await storageStream.CopyToAsync(parseStream, cancellationToken);
            storageStream.Position = 0;
            parseStream.Position = 0;

            // Save PDF
            var url = await _storage.SaveAsync(storageStream, request.FileName, "resumes", cancellationToken);
            student.ResumeUrl = url;
            student.UpdatedAt = DateTime.UtcNow;
            //_studentRepo.Update(student);

            foreach (var old in student.StudentSkills.ToList())
                _studentSkillRepo.Remove(old);

            var rawText = await _parser.ExtractTextAsync(parseStream);
            var cleanText = _parser.PreprocessText(rawText);
            var parseResult = await _parser.ExtractSkillsAsync(cleanText);


            //var existingSkills = await _studentSkillRepo.GetAllAsync(
            //   q => q.Where(ss => ss.StudentId == student.Id), cancellationToken);

            //foreach (var existing in existingSkills)
            //    _studentSkillRepo.Remove(existing);

            // ── Save newly extracted skills to DB ──
            foreach (var suggestion in parseResult.Suggestions)
            {
                await _studentSkillRepo.AddAsync(new StudentSkill
                {
                    StudentId = student.Id,
                    SkillId = suggestion.SkillId,
                    ConfirmedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            _studentRepo.Update(student);
            await _uow.SaveChangesAsync(cancellationToken);

            return parseResult;
        }
        catch (Exception ex)
        {
            // This WILL appear in logs
            throw new Exception($"UploadResume failed: {ex.GetType().Name} — {ex.Message} — {ex.StackTrace}", ex);
        }
        //return await _parser.ExtractSkillsAsync(cleanText);
    }
}

using FinalProject_SeventhSem.Application.Models.Resume;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface IResumeParsingService
{
    /// <summary>
    /// Extracts raw text from a PDF stream using PdfPig (Algorithm 1 input).
    /// Throws BadRequestException if the stream is not a valid PDF.
    /// </summary>
    Task<string> ExtractTextAsync(Stream pdfStream);

    /// <summary>
    /// Algorithm 1: normalises raw text (lowercase, strip special chars, fix broken lines).
    /// </summary>
    string PreprocessText(string rawText);

    /// <summary>
    /// Algorithm 2: Dictionary-Based NER over clean text.
    /// Returns skill suggestions sorted by confidence DESC, capped at MaxSuggestionsCount.
    /// </summary>
    Task<ResumeParseResponse> ExtractSkillsAsync(string cleanText);
}

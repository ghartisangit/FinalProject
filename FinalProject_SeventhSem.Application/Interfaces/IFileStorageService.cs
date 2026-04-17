using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves the stream and returns the public URL/path for the stored file.
    /// </summary>
    Task<string> SaveAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default);

    /// <summary>Deletes a previously stored file by its URL/path.</summary>
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}


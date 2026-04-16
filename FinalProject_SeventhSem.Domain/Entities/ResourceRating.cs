using FinalProject_SeventhSem.Domain.Common;

namespace FinalProject_SeventhSem.Domain.Entities;

/// <summary>
/// Student rating for a learning resource (1–5 stars).
/// One rating per student per resource (enforced by unique index in Infrastructure).
/// </summary>
public class ResourceRating : BaseEntity
{
    public int ResourceId { get; set; }
    public int StudentId { get; set; }

    /// <summary>Rating value between 1 and 5 inclusive.</summary>
    public int Rating { get; set; }

    public string? Comment { get; set; }
    public DateTime RatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Resource Resource { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

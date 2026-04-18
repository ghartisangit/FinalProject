using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(300);
        builder.Property(r => r.Url).IsRequired().HasMaxLength(500);
        builder.Property(r => r.ResourceType).HasMaxLength(50);
        builder.Property(r => r.Description).HasMaxLength(1000);

        builder.HasMany(r => r.SkillMappings)
            .WithOne(m => m.Resource)
            .HasForeignKey(m => m.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Ratings)
            .WithOne(rt => rt.Resource)
            .HasForeignKey(rt => rt.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



public class ResourceRatingConfiguration : IEntityTypeConfiguration<ResourceRating>
{
    public void Configure(EntityTypeBuilder<ResourceRating> builder)
    {
        builder.HasKey(r => r.Id);

        // One rating per student per resource
        builder.HasIndex(r => new { r.ResourceId, r.StudentId }).IsUnique();

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(500);

        builder.HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.TokenHash).IsRequired();
        builder.Property(rt => rt.TokenLookup).IsRequired().HasMaxLength(32);

        // Unique index enables O(1) lookup before BCrypt.Verify
        builder.HasIndex(rt => rt.TokenLookup).IsUnique();

        // Self-referencing FK for rotation chain
        builder.HasOne(rt => rt.ReplacedByToken)
            .WithMany()
            .HasForeignKey(rt => rt.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}





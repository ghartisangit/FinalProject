using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Core auth ──────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── Profiles ───────────────────────────────────────────────────────────
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Organization> Organizations => Set<Organization>();

    // ── Skills ─────────────────────────────────────────────────────────────
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<SkillAlias> SkillAliases => Set<SkillAlias>();
    public DbSet<StudentSkill> StudentSkills => Set<StudentSkill>();

    // ── Vacancies & Applications ───────────────────────────────────────────
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<VacancySkill> VacancySkills => Set<VacancySkill>();
    public DbSet<FinalProject_SeventhSem.Domain.Entities.Application> Applications => Set<FinalProject_SeventhSem.Domain.Entities.Application>();
    public DbSet<ApplicationMatchSnapshot> ApplicationMatchSnapshots => Set<ApplicationMatchSnapshot>();

    // ── Test system ────────────────────────────────────────────────────────
    public DbSet<Stack> Stacks => Set<Stack>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<TestAnswer> TestAnswers => Set<TestAnswer>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<StudentSeenQuestion> StudentSeenQuestions => Set<StudentSeenQuestion>();

    // ── Resources ──────────────────────────────────────────────────────────
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceSkillMapping> ResourceSkillMappings => Set<ResourceSkillMapping>();
    public DbSet<ResourceRating> ResourceRatings => Set<ResourceRating>();

    // ── Audit ──────────────────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}


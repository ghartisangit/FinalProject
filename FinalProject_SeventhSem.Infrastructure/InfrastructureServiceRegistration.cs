using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Domain.Interfaces;
using FinalProject_SeventhSem.Infrastructure.Engines;
using FinalProject_SeventhSem.Infrastructure.Persistence;
using FinalProject_SeventhSem.Infrastructure.Persistence.Interceptors;
using FinalProject_SeventhSem.Infrastructure.Persistence.Repositories;
using FinalProject_SeventhSem.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure;

/// <summary>
/// Registers all Infrastructure services: EF Core, repositories, engines, and utility services.
/// Called from Program.cs: builder.Services.AddInfrastructureServices(builder.Configuration)
/// </summary>
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Audit interceptor (singleton — stateless) ──────────────────────
        services.AddScoped<AuditInterceptor>();

        // ── EF Core DbContext ──────────────────────────────────────────────
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(
                    typeof(InfrastructureServiceRegistration).Assembly.FullName));

            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        // IUnitOfWork is satisfied by AppDbContext (already scoped)
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // ── Generic repository ─────────────────────────────────────────────
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // ── Auth & security services ───────────────────────────────────────
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IJwtService, JwtService>();

        // ── Current user (HTTP-context-bound) ─────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── File storage ───────────────────────────────────────────────────
        var uploadsPath = configuration["FileStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        services.AddScoped<IFileStorageService>(_ => new FileStorageService(uploadsPath));

        // ── Core engines ───────────────────────────────────────────────────
        services.AddScoped<IResumeParsingService, ResumeParsingEngine>();
        services.AddScoped<IMatchingService, MatchingEngine>();
        services.AddScoped<IScoringService, ScoringEngine>();

        // ── Database seeder ────────────────────────────────────────────────
        services.AddScoped<FinalProject_SeventhSem.Infrastructure.Seeders.DatabaseSeeder>();

        return services;
    }
}

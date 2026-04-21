using FinalProject_SeventhSem.Domain.Common;
using FinalProject_SeventhSem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository. Satisfies IRepository&lt;T&gt; for all entities.
/// Feature-specific queries (e.g. filtering by StudentId) are handled in CQRS
/// handlers via GetAllAsync + LINQ — keeping the repo surface minimal.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public async Task<T?> GetByIdAsync(int id,
    Func<IQueryable<T>, IQueryable<T>>? include = null,
    CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _context.Set<T>();

        if (include != null)
            query = include(query);

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(
    Func<IQueryable<T>, IQueryable<T>> include,
    CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _context.Set<T>();
        query = include(query);
        return await query.ToListAsync(cancellationToken);
    }
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);
}


using System.Linq.Expressions;

namespace FormationManagement.Application.Common.Interfaces;

/// <summary>
/// Generic repository abstraction over EF Core, so Application services never
/// depend on DbContext/DbSet directly (keeps them unit-testable and keeps
/// EF Core out of the Application layer entirely).
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>Query with optional filter, include chains, ordering and paging — covers most list-screen needs.</summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null);

    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);
}

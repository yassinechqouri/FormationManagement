using FormationManagement.Domain.Entities;
using FormationManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FormationManagement.Infrastructure.Persistence;

/// <summary>
/// Application database context. Inherits IdentityDbContext so ASP.NET Identity
/// tables (AspNetUsers, AspNetRoles, ...) live in the same database as the
/// domain tables, which is the simplest and most common setup for this kind
/// of project. Also implements IDataProtectionKeyContext so ASP.NET Core's
/// encryption keys (used for antiforgery tokens, auth cookies, etc.) persist
/// in the database rather than the container's local disk — containers on
/// hosts like Railway/Render get a fresh disk on every restart/redeploy,
/// which would otherwise silently invalidate every open session and any
/// in-flight form on each deploy.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys => Set<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // IMPORTANT: must be called first so Identity's own entity configuration is applied.
        base.OnModelCreating(builder);

        // Apply every IEntityTypeConfiguration<T> found in this assembly
        // (Persistence/Configurations/*.cs) instead of configuring everything
        // inline here — keeps this file short and each entity's rules isolated.
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter: hide soft-deleted rows automatically from every
        // LINQ query without needing ".Where(x => !x.IsDeleted)" everywhere.
        builder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Trainer>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Module>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Lesson>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Exercise>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Quiz>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Question>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Answer>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Automatically stamps CreatedAt/UpdatedAt on every save, so services never have to remember to.</summary>
    private void ApplyAuditInfo()
    {
        var entries = ChangeTracker.Entries<Domain.Common.BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}
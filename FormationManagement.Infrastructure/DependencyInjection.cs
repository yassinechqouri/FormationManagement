using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.Common.Models;
using FormationManagement.Application.Interfaces;
using FormationManagement.Application.Services;
using FormationManagement.Infrastructure.Identity;
using FormationManagement.Infrastructure.Persistence;
using FormationManagement.Infrastructure.Repositories;
using FormationManagement.Infrastructure.Services.AIAvatar;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FormationManagement.Infrastructure;

/// <summary>
/// Wires up everything the Infrastructure layer provides. Program.cs only
/// needs to call `builder.Services.AddInfrastructure(builder.Configuration)`
/// — keeps Program.cs short and keeps EF Core/Identity plumbing out of the Web project.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // --- Database ---
        // "DatabaseProvider" in appsettings/env vars picks the EF Core provider:
        //   "SqlServer" (default) - what you run locally against SQL Server Express
        //   "Postgres"             - used for free cloud hosting (e.g. Supabase + Render)
        // This lets local development stay exactly as-is while a cloud deployment
        // targets a free Postgres database without touching any C# code.
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var databaseProvider = configuration["DatabaseProvider"] ?? "SqlServer";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
            else
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
        });

        // --- Identity ---
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password policy — reasonably strong but still usable for a
                // university demo. Tighten for a real production deployment.
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // flip to true once real email sending is configured
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders(); // needed for password-reset / email-confirmation tokens

        // --- Repository pattern ---
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // --- Data Protection key persistence ---
        // Without this, encryption keys (used for antiforgery tokens, auth
        // cookies, etc.) are written to the container's local disk, which is
        // wiped on every restart/redeploy on hosts like Railway/Render —
        // silently breaking any open form/session each time the app redeploys.
        // Storing them in the same database instead makes them durable.
        services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>();

        // --- User directory (Identity, exposed to Application via abstraction) ---
        services.AddScoped<IUserDirectoryService, Services.IdentityUserDirectoryService>();

        // --- Application-layer business services ---
        // Registered here (instead of a separate Application DI extension) so
        // Program.cs only ever needs a single AddInfrastructure(...) call.
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ICourseContentService, CourseContentService>();
        services.AddScoped<IQuizExerciseService, QuizExerciseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // --- AI Avatar provider (swappable via configuration) ---
        services.Configure<AIAvatarOptions>(configuration.GetSection(AIAvatarOptions.SectionName));

        var provider = configuration.GetSection(AIAvatarOptions.SectionName)["Provider"] ?? "HeyGen";
        services.AddHttpClient<IAIAvatarService, HeyGenAvatarService>(); // default registration

        if (provider.Equals("DId", StringComparison.OrdinalIgnoreCase))
        {
            // Overrides the default registration above when Provider = "DId".
            services.AddHttpClient<IAIAvatarService, DIdAvatarService>();
        }

        return services;
    }
}
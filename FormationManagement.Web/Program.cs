using FormationManagement.Infrastructure;
using FormationManagement.Infrastructure.Identity;
using FormationManagement.Infrastructure.Persistence;
using FormationManagement.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------

// Everything EF Core / Identity / Repository / Application-service related
// is registered in one call — see FormationManagement.Infrastructure/DependencyInjection.cs
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<FormationManagement.Web.Services.ExportService>();

builder.Services.AddControllersWithViews(options =>
{
    // CSRF protection is applied globally so every POST/PUT/DELETE action
    // requires a valid anti-forgery token unless explicitly opted out.
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddRazorPages(); // required by Identity's email-confirmation link generation helpers

// QuestPDF community license — required as of QuestPDF 2023.x+.
QuestPDF.Settings.License = LicenseType.Community;

// Cookie/auth redirect paths pointing at our custom AccountController (Module 4),
// instead of the default Identity Razor Pages UI, since the assignment calls
// for MVC controllers + Razor views throughout.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// ---------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseMigrationsEndPoint(); // friendly EF Core migration errors during development
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ---------------------------------------------------------------------
// Apply migrations + seed data automatically on startup.
// Convenient for grading/demoing; disable/replace with a proper release
// pipeline step for a real production deployment.
// ---------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
    if (databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
    {
        // Migrations generated against SQL Server embed SQL Server-specific
        // annotations and won't cleanly replay against Postgres, so the cloud
        // (Postgres) deployment creates its schema directly from the current
        // model instead of applying migration files.
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    await ApplicationDbSeeder.SeedAsync(services);
}

app.Run();

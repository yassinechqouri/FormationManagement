using FormationManagement.Domain.Entities;
using FormationManagement.Domain.Enums;
using FormationManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FormationManagement.Infrastructure.Seed;

/// <summary>
/// Runs once on startup (see Program.cs) to guarantee the three roles exist
/// and to create a default admin account + a handful of demo entities, so the
/// app is immediately usable/demoable after `dotnet ef database update`.
/// </summary>
public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<Persistence.ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ApplicationDbSeeder");

        // 1. Roles
        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. Default Administrator account
        const string adminEmail = "admin@formation.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            // NOTE: change this password immediately after first login in any
            // non-classroom deployment. It is intentionally strong-but-known
            // only to satisfy Identity's default password policy for the demo.
            var result = await userManager.CreateAsync(adminUser, "Admin@12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
            else
                logger.LogWarning("Could not create seed admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // 3. Demo Trainer account + Trainer profile
        const string trainerEmail = "trainer@formation.local";
        var trainerUser = await userManager.FindByEmailAsync(trainerEmail);
        if (trainerUser is null)
        {
            trainerUser = new ApplicationUser
            {
                UserName = trainerEmail,
                Email = trainerEmail,
                FirstName = "Jane",
                LastName = "Doe",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(trainerUser, "Trainer@12345");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(trainerUser, ApplicationRoles.Trainer);

                context.Trainers.Add(new Trainer
                {
                    ApplicationUserId = trainerUser.Id,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = trainerEmail,
                    Biography = "Full-stack instructor with 10 years of industry experience."
                });
                await context.SaveChangesAsync();
            }
        }

        // 4. Demo Learner account
        const string learnerEmail = "learner@formation.local";
        if (await userManager.FindByEmailAsync(learnerEmail) is null)
        {
            var learnerUser = new ApplicationUser
            {
                UserName = learnerEmail,
                Email = learnerEmail,
                FirstName = "Sam",
                LastName = "Learner",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(learnerUser, "Learner@12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(learnerUser, ApplicationRoles.Learner);
        }

        // 5. Sample categories
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Web Development", Description = "Front-end and back-end web technologies." },
                new Category { Name = "Data Science", Description = "Data analysis, machine learning and statistics." },
                new Category { Name = "Cloud & DevOps", Description = "Cloud platforms, CI/CD and infrastructure." }
            );
            await context.SaveChangesAsync();
        }

        // 6. One sample course, so the catalog isn't empty on first run
        if (!context.Courses.Any())
        {
            var webCategory = context.Categories.First(c => c.Name == "Web Development");
            var trainer = context.Trainers.FirstOrDefault();

            if (trainer != null)
            {
                var course = new Course
                {
                    Title = "ASP.NET Core MVC From Scratch",
                    Description = "Learn to build production-ready web apps with ASP.NET Core MVC, EF Core and Identity.",
                    CategoryId = webCategory.Id,
                    TrainerId = trainer.Id,
                    Level = CourseLevel.Beginner,
                    Price = 49.99m,
                    Duration = 600,
                    Published = true
                };

                course.Modules.Add(new Domain.Entities.Module
                {
                    Title = "Getting Started",
                    Description = "Environment setup and project structure.",
                    Order = 1,
                    Lessons =
                    {
                        new Lesson { Title = "Installing the .NET SDK", Duration = 15 },
                        new Lesson { Title = "Creating your first MVC project", Duration = 25 }
                    }
                });

                context.Courses.Add(course);
                await context.SaveChangesAsync();
            }
        }
    }
}

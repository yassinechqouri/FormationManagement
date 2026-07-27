# Formation Management System

An ASP.NET Core 8 MVC application for managing online training courses:
Administrators run the platform, Trainers author courses, and Learners
browse, enroll, and study — with an AI avatar that presents each lesson and
answers questions live.

## Architecture

Clean Architecture, 4 projects:

```
FormationManagement.Domain          Entities, enums — zero external dependencies
FormationManagement.Application     DTOs, service interfaces + implementations, business logic
FormationManagement.Infrastructure  EF Core, Identity, Repository/UnitOfWork, AI avatar providers
FormationManagement.Web             MVC controllers, Razor views, REST API, PDF/Excel export
```

Dependencies flow inward only: Web → Infrastructure → Application → Domain.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB is fine for development — ships with Visual Studio, or
  install "SQL Server Express LocalDB" standalone)
- (Optional) An API key from a supported AI avatar provider — HeyGen, D-ID,
  Anam AI, or Azure Avatar — to see real avatar video instead of the built-in
  placeholder response.

> **Note on this delivery**: this codebase was written and reviewed by hand
> in an environment without a working .NET SDK, so it has **not** been
> compiled here. Follow the steps below on a machine with the .NET 8 SDK to
> build it, generate the real EF Core migration, and confirm everything
> compiles before you rely on it for grading/demoing.

## First-time setup

```bash
cd FormationManagement

# 1. Restore all 4 projects
dotnet restore

# 2. Point the connection string at your SQL Server instance
#    (edit FormationManagement.Web/appsettings.json, or better, use user-secrets:)
cd FormationManagement.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=FormationManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
cd ..

# 3. Generate the initial migration (this repo intentionally ships without
#    pre-generated Migrations/*.cs files, since those are machine-generated
#    and must match your exact EF Core tooling version)
dotnet tool install --global dotnet-ef   # if you don't already have it
dotnet ef migrations add InitialCreate \
  --project FormationManagement.Infrastructure \
  --startup-project FormationManagement.Web

# 4. Build
dotnet build

# 5. Run (this applies the migration + seeds demo data automatically on startup)
dotnet run --project FormationManagement.Web
```

The app will be available at the URL printed in the console (typically
`https://localhost:5001` or similar). Migrations and seed data run
automatically in `Program.cs` on every startup — no separate `database
update` step needed after step 3.

## Demo accounts (created by the seeder)

| Role          | Email                      | Password        |
|---------------|----------------------------|------------------|
| Administrator | admin@formation.local      | Admin@12345      |
| Trainer       | trainer@formation.local    | Trainer@12345    |
| Learner       | learner@formation.local    | Learner@12345    |

## Configuring the AI Avatar provider

Edit `FormationManagement.Web/appsettings.json` (or user-secrets in
development):

```json
"AIAvatar": {
  "Provider": "HeyGen",
  "BaseUrl": "https://api.heygen.com/",
  "ApiKey": "your-api-key",
  "AvatarId": "your-avatar-id",
  "VoiceId": "your-voice-id"
}
```

Switching provider: set `Provider` to `"DId"` to use the D-ID implementation
instead (`FormationManagement.Infrastructure/Services/AIAvatar/DIdAvatarService.cs`).
To add Anam AI or Azure Avatar, implement `IAIAvatarService`
(`FormationManagement.Application/Interfaces/IAIAvatarService.cs`) following
the same pattern as the HeyGen/D-ID classes, then register it in
`FormationManagement.Infrastructure/DependencyInjection.cs`. No other file in
the solution needs to change.

Without a real API key, lesson pages still work — `HeyGenAvatarService` will
fail the HTTP call gracefully and the lesson page shows the fallback caption
text instead of a video.

## Testing the application manually

1. **Register a Learner**: go to `/Account/Register`, create an account.
2. **Browse & enroll**: `/Course` → search/filter/sort → open a course →
   "Enroll Now".
3. **Take a lesson**: from the course details page, click "Start" on a
   lesson — see the AI trainer panel, ask it a question, take any attached
   quiz.
4. **Log in as Trainer** (`trainer@formation.local` / `Trainer@12345`):
   `/Trainer` → create a course → "Manage Content" → add modules, lessons,
   exercises, quizzes and questions.
5. **Log in as Administrator** (`admin@formation.local` / `Admin@12345`):
   `/Admin` (or the "Admin Dashboard" nav link) → see the 5 stat cards and
   recent enrollments → explore Manage Users / Trainers / Categories /
   Courses / Enrollments → `/Admin/Reports` → download Excel/PDF exports.
6. **REST API**: `GET /api/courses`, `GET /api/courses/{id}`,
   `GET /api/dashboard/stats` (admin only), `POST /api/enrollments` (learner
   only, JSON body `{"courseId": 1}`).

## Automated testing

No test project is included in this delivery (out of scope for the brief,
which asked for the application itself). To add one:

```bash
dotnet new xunit -n FormationManagement.Tests
dotnet add FormationManagement.Tests reference FormationManagement.Application
dotnet add FormationManagement.Tests package Moq
dotnet sln add FormationManagement.Tests
```

Because business logic lives in `FormationManagement.Application/Services/*`
behind `IUnitOfWork`/`IGenericRepository` interfaces, each service
(`CourseService`, `EnrollmentService`, `QuizExerciseService`, etc.) can be
unit-tested by mocking `IUnitOfWork` with Moq — no database required.

## Deploying to Railway

This repo includes a `Dockerfile` and `railway.toml` at the solution root.

1. Push this repository to GitHub.
2. In Railway, "New Project" → "Deploy from GitHub repo" → select it.
   Railway will detect `railway.toml` and build using the Dockerfile.
3. Provision a SQL Server-reachable database (Railway doesn't host SQL
   Server natively — use Azure SQL, or switch the EF Core provider to
   Postgres and use Railway's built-in Postgres plugin — see the note in
   `railway.toml`).
4. In Railway's **Variables** tab, set:
   - `ConnectionStrings__DefaultConnection`
   - `AIAvatar__Provider`, `AIAvatar__BaseUrl`, `AIAvatar__ApiKey`, `AIAvatar__AvatarId`, `AIAvatar__VoiceId`
   - `ASPNETCORE_ENVIRONMENT=Production`
5. Deploy. Migrations + seed data apply automatically on startup (see the
   bottom of `Program.cs`).

## Security notes

- CSRF: `AutoValidateAntiforgeryTokenAttribute` is applied globally in
  `Program.cs`, plus explicit `[ValidateAntiForgeryToken]` on every
  state-changing action.
- Passwords: hashed by ASP.NET Core Identity (PBKDF2), never stored or
  logged in plain text.
- Authorization: every Admin/Trainer/Learner-only action carries an explicit
  `[Authorize(Roles = ...)]` attribute; ownership checks (e.g. a Trainer
  editing only their own courses) are enforced in the controller before any
  service call.
- Soft delete: all entities use `IsDeleted` + a global EF Core query filter,
  so historical enrollment/progress data survives a course/trainer/category
  being "deleted".

## Known gaps to close before a real production launch

- Wire up a real email provider (SendGrid/SMTP) for password-reset links —
  currently logged to the console/`ILogger` for demo purposes.
- Add rate limiting to the public REST API endpoints.
- Add integration/unit tests (see above).
- Tighten `options.Password.RequireNonAlphanumeric` etc. in
  `FormationManagement.Infrastructure/DependencyInjection.cs` if a stronger
  policy is required.

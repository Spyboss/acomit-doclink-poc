# Local Development Guide

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (14+) — or a [Supabase](https://supabase.com) project
- [Git](https://git-scm.com/)
- (Optional) [Railway CLI](https://docs.railway.com/guides/cli) — for deployment

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/Spyboss/acomit-doclink-poc.git
cd acomit-doclink-poc/DocLink
```

### 2. Configure the database connection

Create a PostgreSQL database and set the connection string via environment variable:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=doclink;Username=postgres;Password=postgres;SSL Mode=Disable;"
```

Or create an `appsettings.Production.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=doclink;Username=postgres;Password=postgres;"
  }
}
```

For **Supabase** (remote PostgreSQL), use:

```bash
export ConnectionStrings__DefaultConnection="Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.YOUR_PROJECT;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;"
```

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Run the application

```bash
dotnet run
```

The app starts at `http://localhost:5088` by default.

**Migrations apply automatically** on startup via `db.Database.Migrate()` in `Program.cs:54`.

## Development Workflow

### Creating a new migration

If you change the `Document` entity or add new entities:

```bash
dotnet ef migrations add MigrationName
```

The `Microsoft.EntityFrameworkCore.Design` package is already included in the project.

### Viewing applied migrations

```bash
dotnet ef migrations list
```

### Reverting a migration

```bash
dotnet ef database update PreviousMigrationName
```

### Running with hot reload

```bash
dotnet watch run
```

## Testing SMS

The `MockSmsService` logs SMS messages to the console. To see dispatched SMS:

```bash
dotnet run
# Watch the console output when you submit the form:
# --- SMS Dispatch ---
# To: +9471XXXXXXX
# Message: Your Receipt is ready.\n\nDocLink\nhttp://...
# --------------------
```

To test with a real SMS provider, implement `ISmsService` (e.g., with Twilio) and swap the DI registration in `Program.cs`:

```csharp
// builder.Services.AddScoped<ISmsService, MockSmsService>();
builder.Services.AddScoped<ISmsService, TwilioSmsService>();
```

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Yes | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | No | `Development` or `Production` (default: `Production`) |
| `ASPNETCORE_URLS` | No | Override binding URL (default: `http://localhost:5088`) |
| `PORT` | No | Port override (used by Railway) |

The `__` (double underscore) is the .NET convention for hierarchical config keys in environment variables.

## Project Structure Reference

```
DocLink/
├── Controllers/       # MVC controllers
├── Data/             # EF Core DbContext
├── Migrations/       # EF migrations
├── Models/           # Domain entities + enums
├── Services/         # Business logic
├── ViewModels/       # View models with validation
├── Views/            # Razor templates
├── wwwroot/          # Static files (Bootstrap, jQuery)
├── Program.cs        # Entry point
├── Dockerfile        # Container build
└── DocLink.csproj    # Project file
```

## Common Commands

```bash
dotnet restore          # Restore NuGet packages
dotnet build            # Build the project
dotnet run              # Run the app
dotnet watch run        # Run with hot reload
dotnet publish -c Release -o ./publish  # Publish for deployment
dotnet ef migrations list                # List migrations
dotnet ef migrations add Name            # Create new migration
```

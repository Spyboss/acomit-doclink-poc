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

### Using Supabase (Remote PostgreSQL)

Supabase provides a managed PostgreSQL database that works well with this project.

**Setup:**
1. Create a free project at [supabase.com](https://supabase.com)
2. Go to **Project Settings → Database → Connection string**
3. Copy the **Session pooler** string (port 5432, not port 6543)

**Connection string format:**

```
Host=aws-0-{region}.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.{project_ref};Password={password};SSL Mode=Require;Trust Server Certificate=true;
```

**Set it:**

```bash
export ConnectionStrings__DefaultConnection="Host=aws-0-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.qoyeqfwxiofcvwdirwrg;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;"
```

> **Important:** Use port **5432** (session mode) not 6543 (transaction mode). EF Core migrations and design-time commands will time out on port 6543.

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

## Testing WhatsApp

The `MockMessagingService` logs WhatsApp messages to the console. To see dispatched messages:

```bash
dotnet run
# Watch the console output when you submit the form:
# --- WhatsApp Message Dispatch ---
# To: +9471XXXXXXX
# Message: Your Receipt is ready.\n\nDocLink\nhttp://...
# ---------------------------------
```

To send real WhatsApp messages, see the [Messaging Provider Guide](messaging-providers.md). It covers:

- **Meta WhatsApp Cloud API** — free for utility/receipt messages, full C# implementation included
- **Local Sri Lankan providers** — Bloomwire, Go4whatsup, Notify.lk, Text.lk
- **International alternatives** — Twilio WhatsApp, MessageBird, Vonage, WATI

Example swap in `Program.cs`:

```csharp
// builder.Services.AddScoped<IMessagingService, MockMessagingService>();
builder.Services.AddScoped<IMessagingService, WhatsAppCloudApiService>();
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

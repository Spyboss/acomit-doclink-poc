# Deployment Guide

## Railway Deployment

The app is deployed on Railway at **[https://acomit-doclink-poc-production.up.railway.app](https://acomit-doclink-poc-production.up.railway.app)**.

### Prerequisites

- A [Railway](https://railway.app) account
- [Railway CLI](https://docs.railway.com/guides/cli) (optional)
- A PostgreSQL database (the app uses [Supabase](https://supabase.com))

### Required Files

These files are already in the repository:

| File | Purpose |
|------|---------|
| `DocLink/Dockerfile` | Multi-stage .NET 8 build (SDK → runtime) |
| `DocLink/.dockerignore` | Excludes bin/, obj/, .git, .vs from image |
| `DocLink/Program.cs` | Reads `PORT` env var, exposes `/health` endpoint |

**Note:** Railway does not support automatic .NET detection (Railpack). A `Dockerfile` is required.

### Environment Variables

Set these in the Railway dashboard under your service's **Variables** tab:

| Variable | Value | Purpose |
|----------|-------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Enables production error handling |
| `ConnectionStrings__DefaultConnection` | `Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true;` | PostgreSQL connection string (use Supabase) |
| `ASPNETCORE_URLS` | `http://0.0.0.0:${PORT}` | Bind to all interfaces on Railway-assigned port |

The `PORT` variable is automatically set by Railway.

### Deploy via GitHub (Recommended)

1. Push the repository to GitHub
2. In the Railway dashboard: **New Project → Deploy from GitHub repo**
3. Select your repository (`Spyboss/acomit-doclink-poc`)
4. In the service **Settings → Root Directory**, set it to `DocLink`
5. Add the environment variables (see above)
6. Railway auto-detects `Dockerfile` and builds/deploys
7. Go to **Networking → Generate Domain** for a public URL

### Deploy via CLI

```bash
npm install -g @railway/cli
railway login
cd DocLink
railway link --project your-project-name
railway up
```

### Post-Deployment

1. Verify the app is running:
   ```bash
   curl https://your-domain.up.railway.app/health
   # {"status":"healthy"}
   ```
2. Generate a public domain in the **Networking** tab if not already set
3. Monitor logs:
   ```bash
   railway logs
   ```

### Redeploying

After pushing new commits to GitHub, Railway auto-deploys (if autodeploy is enabled). To manually trigger:

```bash
railway redeploy
```

### Health Check

The app exposes a `/health` endpoint that returns HTTP 200:

```json
{"status":"healthy"}
```

Railway uses this (if `railway.json` is configured) to verify the app is running.

## Docker Deployment (Generic)

If deploying to any Docker host:

```bash
# Build the image
docker build -t doclink DocLink/

# Run with PostgreSQL connection string
docker run -d -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=..." \
  -e ASPNETCORE_URLS="http://0.0.0.0:8080" \
  doclink
```

## Database

The app uses **Supabase PostgreSQL** in production. The connection string format:

```
Host=<host>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true;
```

Migrations run **automatically on startup**. No manual migration steps are needed.

> **Warning:** Do not store the connection string in `appsettings.json` or version control. Always use environment variables in production.

## Switching to a Real SMS Provider

The app currently uses `MockSmsService` (logs to console). To send real SMS:

1. Add your SMS provider's NuGet package (e.g., `Twilio`)
2. Create a new class implementing `ISmsService`
3. In `Program.cs`, swap the DI registration:
   ```csharp
   // builder.Services.AddScoped<ISmsService, MockSmsService>();
   builder.Services.AddScoped<ISmsService, TwilioSmsService>();
   ```
4. Add provider credentials as environment variables

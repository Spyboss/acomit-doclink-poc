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

## Supabase Setup

This app uses **Supabase** as its PostgreSQL provider. To set up your own:

### 1. Create a Supabase project

1. Go to [supabase.com](https://supabase.com) and sign in
2. Click **New Project**
3. Enter a name (e.g., `doclink`)
4. Set a **database password** (save this securely — it cannot be recovered)
5. Choose a **region** closest to your users (e.g., `ap-northeast-2` for Seoul, nearby for Sri Lanka)
6. Click **Create new project** (takes 1–2 minutes to provision)

### 2. Get the connection string

1. In the Supabase dashboard, go to **Project Settings → Database**
2. Under **Connection string**, find the **URI** or **Params** tab
3. Copy the connection string. There are two pooler modes:

| Mode | Port | Use Case |
|------|------|----------|
| **Transaction** (pooler) | 6543 | For serverless/runtime queries (short-lived connections) |
| **Session** / **Direct** | 5432 | For migrations, EF Core design-time tools |

For EF Core (this project), use **Session mode on port 5432**:

```
Host=aws-0-{region}.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.{project_ref};Password={password};SSL Mode=Require;Trust Server Certificate=true;
```

> **Note:** As of Feb 2025, port 6543 is transaction-mode only and may cause migration timeouts with EF Core. Always use port 5432 for `dotnet ef` commands and runtime if you encounter issues.

### 3. Configure Railway

Set the connection string as the Railway env var `ConnectionStrings__DefaultConnection` (see Environment Variables above).

### 4. Migrations

EF Core migrations run **automatically on startup** via `db.Database.Migrate()` in `Program.cs`. No manual migration steps on deploy.

> **Warning:** Never commit the connection string to version control. Always use environment variables in production.

## SMS Providers

For real SMS dispatch, see the [SMS Provider Comparison](sms-providers.md) guide covering:

- **Twilio** — $0.42/msg to Sri Lanka, 1 msg/sec per long code
- **Local providers** — Text.lk (0.64 LKR/msg), FITSMS (0.68 LKR/msg), Notify.lk, etc.
- **International alternatives** — Plivo, Vonage, MessageBird, AWS SNS
- Full implementation guide with code samples

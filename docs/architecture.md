# Architecture

## System Overview

DocLink is a monolithic ASP.NET Core MVC application with server-rendered Razor views and a PostgreSQL database backend. It follows the standard MVC pattern with a service layer for business logic.

```
┌──────────────┐    HTTP    ┌──────────────────────────────────────┐
│   Browser    │ ◄────────► │         ASP.NET Core MVC            │
│  (User/Dev)  │            │                                      │
└──────────────┘            │  ┌────────┐  ┌──────────────────┐   │
                            │  │Controllers│  │   Services     │   │
┌──────────────┐            │  │  ┌─────┐ │  │  ┌───────────┐  │   │
│   Browser    │    HTTP    │  │  │Document│ │  │TokenService│  │   │
│  (Customer)  │ ◄────────► │  │  │.cs    │ │  │Document   │  │   │
└──────────────┘            │  │  ├─────┤ │  │Service    │  │   │
                            │  │  │Public │ │  ├───────────┤  │   │
┌──────────────┐            │  │  │.cs    │ │  │PdfService │  │   │
│   SMS App    │   (mock)   │  │  ├─────┤ │  ├───────────┤  │   │
│  (Customer)  │ ◄──────────┤  │  │Home  │ │  │SmsService │  │   │
└──────────────┘            │  │  │.cs   │ │  └───────────┘  │   │
                            │  └──┴─────┘ ┘                  │   │
                            │         │                       │   │
                            │         ▼                       │   │
                            │  ┌────────────┐                 │   │
                            │  │AppDbContext│ ◄─── EF Core ───┤   │
                            │  └─────┬──────┘                 │   │
                            └────────┼───────────────────────────┘
                                     │
                                     ▼
                            ┌────────────────┐
                            │   PostgreSQL   │
                            │  (Supabase)    │
                            └────────────────┘
```

## Layers

### 1. Presentation Layer (Views + ViewModels)

- **Razor Views** (`.cshtml`) — server-rendered HTML with Bootstrap 5 styling
- **ViewModels** — strongly-typed models with DataAnnotation validation
  - `CreateDocumentViewModel` — form input with `[Required]`, `[Phone]`, `[Range]`, `[MaxLength]`
  - `PublicDocumentViewModel` — read-only display model for public receipt page
- **Layout** — shared `_Layout.cshtml` with Bootstrap CDN, jQuery, nav bar, footer

### 2. Controller Layer

Three controllers:

| Controller | Responsibilities |
|------------|-----------------|
| `HomeController` | Root redirect (`/` → `/Document/Create`), error page |
| `DocumentController` | Receipt creation (GET/POST), success page |
| `PublicController` | Token-based public receipt view, PDF download |

All controllers use constructor-injected services. Rate limiting is applied via `[EnableRateLimiting]` attributes.

### 3. Service Layer

Five services, each backed by an interface for testability:

| Service | Interface | Responsibility |
|---------|-----------|---------------|
| `TokenService` | `ITokenService` | Generate cryptographically random tokens (`RandomNumberGenerator.GetString`) |
| `DocumentService` | `IDocumentService` | Create documents, look up by public token |
| `PdfService` | `IPdfService` | Generate A4 PDF receipts using QuestPDF |
| `MockSmsService` | `ISmsService` | Mock SMS dispatch (logs to `ILogger`) |

### 4. Data Layer

- **AppDbContext** — Entity Framework Core context configured for PostgreSQL
- **Migrations** — auto-applied on startup via `db.Database.Migrate()`
- **Document entity** — single-table design with unique index on `PublicToken`

## Request Flow: Create Receipt

```
1. GET /Document/Create
   └─► DocumentController.Create()
       └─► Returns Create.cshtml with empty CreateDocumentViewModel

2. User fills form → POST /Document/Create
   └─► DocumentController.Create(model)
       ├─► Validates model
       ├─► DocumentService.CreateDocumentAsync(model)
       │   ├─► TokenService.GenerateToken() → random 10-char string
       │   └─► Saves Document to PostgreSQL via EF Core
       ├─► Constructs public URL + SMS message
       ├─► SmsService.SendAsync(phone, message) → logs to console
       └─► Redirects to /Document/Success

3. GET /Document/Success
   └─► DocumentController.Success()
       └─► Shows success page with public URL + SMS preview
```

## Request Flow: View Receipt

```
1. Customer opens /r/{token} (via SMS link)
   └─► PublicController.Index(token)
       ├─► DocumentService.GetByTokenAsync(token)
       ├─► If null → 404
       └─► Returns Public/Index.cshtml with receipt details

2. Customer clicks "Download PDF"
   └─► GET /r/{token}/pdf
       └─► PublicController.Pdf(token)
           ├─► DocumentService.GetByTokenAsync(token)
           ├─► PdfService.GenerateReceiptPdf(document) → byte[]
           └─► Returns PDF file download
```

## Rate Limiting

Configured in `Program.cs` using ASP.NET Core's built-in rate limiter:

| Policy | Endpoints | Limit | Window |
|--------|-----------|-------|--------|
| `CreateDocument` | POST /Document/Create | 5 requests | 1 minute per IP |
| `PublicRead` | GET /r/{token}, GET /r/{token}/pdf | 30 requests | 1 minute per IP |

Excess requests return HTTP 429 (Too Many Requests).

## Security

- **Token-based access** — Each document gets a unique random token (10 chars, alphanumeric) via `RandomNumberGenerator.GetString`. Tokens are unguessable and stored with a unique database index.
- **Anti-forgery tokens** — POST forms use `[ValidateAntiForgeryToken]`
- **Rate limiting** — Prevents brute-force token enumeration and form abuse
- **Data protection warning** — Keys stored in container-local directory (ephemeral); acceptable for PoC
- **No auth** — This PoC has no user authentication; any token holder can view the document

## Configuration

Configuration is loaded through the standard .NET hierarchy: `appsettings.json` → `appsettings.{Environment}.json` → environment variables.

Key config sections:
- `CompanyBranding` — Name, Address, Phone (used in PDF header)
- `ConnectionStrings:DefaultConnection` — PostgreSQL connection string

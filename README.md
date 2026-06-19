# DocLink — Document Delivery Proof of Concept

A secure, token-based document/receipt delivery system built with **ASP.NET Core MVC (.NET 8)** and **PostgreSQL**. Create receipts, share them via SMS with a unique public link, and download PDFs — all with rate-limited endpoints and automated database migrations.

## Live Demo

**[https://acomit-doclink-poc-production.up.railway.app](https://acomit-doclink-poc-production.up.railway.app)**

## Quick Start

```bash
# Prerequisites: .NET 8 SDK, PostgreSQL

# 1. Clone and navigate
git clone https://github.com/Spyboss/acomit-doclink-poc.git
cd acomit-doclink-poc/DocLink

# 2. Set the connection string
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=doclink;Username=postgres;Password=postgres"

# 3. Run (migrations apply automatically)
dotnet run
```

Open `http://localhost:5088` in your browser.

## Features

- **Receipt creation form** with validation (customer name, phone, invoice, amount, date)
- **Public sharing** via cryptographically random token URLs (`/r/{token}`)
- **PDF generation** with QuestPDF — branded A4 receipts with company header and line items
- **SMS dispatch** (mock) — logs to console; swappable to Twilio via `ISmsService`
- **Rate limiting** — 5 req/min for creation, 30 req/min for public views
- **Auto-migrations** — database schema applied on startup
- **Health check** endpoint at `/health`

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8.0 (C#) |
| Framework | ASP.NET Core MVC (Razor) |
| ORM | Entity Framework Core 8.0.11 |
| Database | PostgreSQL (via Npgsql 8.0.11) |
| PDF | QuestPDF 2026.6 (Community) |
| Frontend | Bootstrap 5, jQuery 3 |
| SMS | Mock (ILogger) — pluggable via `ISmsService` |

## Project Structure

```
acomit-doclink-poc/
├── DocLink/                          # ASP.NET Core MVC project
│   ├── Controllers/
│   │   ├── DocumentController.cs     # Receipt creation & success flow
│   │   ├── HomeController.cs         # Root redirect & error handling
│   │   └── PublicController.cs       # Token-based public access + PDF
│   ├── Data/
│   │   └── AppDbContext.cs           # EF Core context (PostgreSQL)
│   ├── Migrations/                   # Auto-generated EF migrations
│   ├── Models/
│   │   ├── Document.cs               # Core entity
│   │   ├── DocumentType.cs           # Enum: Receipt = 1
│   │   ├── ErrorViewModel.cs
│   │   └── Configuration/
│   │       └── CompanyBranding.cs    # Branding config model
│   ├── Services/
│   │   ├── ITokenService.cs / TokenService.cs       # Random token generation
│   │   ├── IDocumentService.cs / DocumentService.cs # Business logic
│   │   ├── IPdfService.cs / PdfService.cs           # QuestPDF generation
│   │   └── ISmsService.cs / MockSmsService.cs       # Mock SMS dispatch
│   ├── ViewModels/
│   │   ├── CreateDocumentViewModel.cs
│   │   └── PublicDocumentViewModel.cs
│   ├── Views/
│   │   ├── Document/
│   │   │   ├── Create.cshtml         # Receipt creation form
│   │   │   └── Success.cshtml        # Success page with SMS preview
│   │   ├── Public/
│   │   │   └── Index.cshtml          # Public receipt view
│   │   └── Shared/
│   │       ├── _Layout.cshtml
│   │       └── Error.cshtml
│   ├── Program.cs                    # Entry point (DI, middleware, routes)
│   ├── appsettings.json              # Config: logging, branding
│   ├── Dockerfile                    # Multi-stage .NET 8 image
│   └── DocLink.csproj
├── docs/                             # Documentation
└── README.md
```

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture](docs/architecture.md) | System design, layers, data flow |
| [ERD](docs/erd.md) | Database schema and entity relationships |
| [Local Dev](docs/local-dev.md) | Setup guide for local development |
| [Deployment](docs/deployment.md) | Railway deployment guide |
| [API Reference](docs/api-reference.md) | All routes, rate limits, request/response |

## Routes

| Method | Path | Rate Limit | Description |
|--------|------|------------|-------------|
| GET | `/` | — | Redirects to /Document/Create |
| GET | `/Document/Create` | — | Receipt creation form |
| POST | `/Document/Create` | 5/min/IP | Submit receipt + dispatch SMS |
| GET | `/Document/Success` | — | Success page with SMS preview |
| GET | `/r/{token}` | 30/min/IP | Public receipt view |
| GET | `/r/{token}/pdf` | 30/min/IP | Download receipt PDF |
| GET | `/health` | — | Health check |

## Deployment

The app is containerized with Docker and deployed on Railway. See [deployment guide](docs/deployment.md) for details.

## License

Proof of Concept — internal use.

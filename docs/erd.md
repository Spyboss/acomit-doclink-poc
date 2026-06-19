# Entity Relationship Diagram

## Current Schema

DocLink has a single table: `Documents`.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Documents                                  │
├───────────────────────────────┬──────────────┬─────────────────────────┤
│ Column                        │ Type         │ Constraints             │
├───────────────────────────────┼──────────────┼─────────────────────────┤
│ Id                            │ uuid         │ PRIMARY KEY             │
│ DocumentType                  │ integer      │ NOT NULL, DEFAULT 1     │
│ DocumentNumber                │ varchar(100) │ NOT NULL                │
│ CustomerName                  │ varchar(200) │ NOT NULL                │
│ PhoneNumber                   │ varchar(50)  │ NOT NULL                │
│ Amount                        │ numeric(18,2)│ NOT NULL                │
│ Date                          │ timestamptz  │ NOT NULL                │
│ Status                        │ varchar(50)  │ NOT NULL                │
│ PublicToken                   │ varchar(100) │ NOT NULL, UNIQUE INDEX  │
│ CreatedAt                     │ timestamptz  │ NOT NULL, DEFAULT NOW() │
│ UpdatedAt                     │ timestamptz  │ NOT NULL, DEFAULT NOW() │
└───────────────────────────────┴──────────────┴─────────────────────────┘
```

## Entity Definition

### `Document`

Represents a single receipt or document issued to a customer. Each document is identified by a unique `PublicToken` used in the shareable link.

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `Guid` | Primary key, generated server-side |
| `DocumentType` | `DocumentType` (enum → int) | Currently only `Receipt = 1` |
| `DocumentNumber` | `string(100)` | Invoice/receipt number from the form |
| `CustomerName` | `string(200)` | Customer's full name |
| `PhoneNumber` | `string(50)` | Phone number for SMS delivery |
| `Amount` | `decimal(18,2)` | Receipt total amount |
| `Date` | `DateTime` (UTC) | Receipt date (normalized to UTC) |
| `Status` | `string(50)` | Default `"Created"` |
| `PublicToken` | `string(100)` | Unique random token (generated via `RandomNumberGenerator`) |
| `CreatedAt` | `DateTime` | Auto-set to `NOW()` on insert |
| `UpdatedAt` | `DateTime` | Auto-set to `NOW()` on insert; updated on modification |

## Indexes

| Index Name | Column(s) | Type |
|-----------|-----------|------|
| `IX_Documents_PublicToken` | `PublicToken` | Unique |

## SQL Definition

```sql
CREATE TABLE "Documents" (
    "Id" uuid NOT NULL,
    "DocumentType" integer NOT NULL DEFAULT 1,
    "DocumentNumber" character varying(100) NOT NULL,
    "CustomerName" character varying(200) NOT NULL,
    "PhoneNumber" character varying(50) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Status" character varying(50) NOT NULL,
    "PublicToken" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Documents_PublicToken" ON "Documents" ("PublicToken");
```

## EF Core Mapping (`AppDbContext.OnModelCreating`)

```csharp
modelBuilder.Entity<Document>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.PublicToken).IsUnique();
    entity.Property(e => e.DocumentType).HasConversion<int>();
    entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
    entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
    entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
});
```

## Migration History

| Migration | Description |
|-----------|-------------|
| `20260619044609_Initial` | Creates `Documents` table with initial schema (Type, Title, Address, Notes, ReferenceNumber columns) |
| `20260619050031_UpdateSchema` | Drops legacy columns (Type, Title, Address, Notes, ReferenceNumber); adds `DocumentType` (int) |

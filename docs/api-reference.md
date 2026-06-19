# API Reference

## Routes

### GET `/` or `/Document/Create`

Displays the receipt creation form.

**Response:** HTML page with form fields.

### POST `/Document/Create`

Submits a new receipt.

**Rate Limit:** 5 requests per minute per IP

**Request Body (form-encoded):**

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `CustomerName` | string | Yes | Max 200 characters |
| `PhoneNumber` | string | Yes | Must match `[Phone]` attribute, max 50 chars |
| `InvoiceNumber` | string | Yes | Max 100 characters |
| `Amount` | decimal | Yes | Between 0.01 and 99,999,999.99 |
| `Date` | date | Yes | Valid date |

**Success Response:** HTTP 302 redirect to `/Document/Success`

**Failure Response:** HTTP 200 with validation errors displayed on the form.

**Sample form submission (curl):**

```bash
curl -X POST https://acomit-doclink-poc-production.up.railway.app/Document/Create \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "CustomerName=John+Doe" \
  -d "PhoneNumber=%2B94711234567" \
  -d "InvoiceNumber=INV-001" \
  -d "Amount=150.00" \
  -d "Date=2026-06-19" \
  -b cookies.txt -c cookies.txt
```

Note: Requires anti-forgery token for actual form submission.

### GET `/Document/Success`

Shows the success page after a receipt is created.

**Query params:** None (data passed via `TempData`)

**Response:** HTML page with receipt details, public URL, PDF download link, and SMS preview.

### GET `/r/{token}`

Views a public receipt.

**Rate Limit:** 30 requests per minute per IP

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `token` | string | The 10-character public token |

**Response:** HTML page displaying the receipt.

**Error:** HTTP 404 if token not found.

**Sample:**

```bash
curl https://acomit-doclink-poc-production.up.railway.app/r/aB3xK9mP2Q
```

### GET `/r/{token}/pdf`

Downloads a receipt as PDF.

**Rate Limit:** 30 requests per minute per IP

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `token` | string | The 10-character public token |

**Response:** `application/pdf` file download with filename `{DocumentNumber}.pdf`

**Error:** HTTP 404 if token not found.

**Sample:**

```bash
curl -O https://acomit-doclink-poc-production.up.railway.app/r/aB3xK9mP2Q/pdf
```

### GET `/health`

Health check endpoint.

**Response:**

```json
{"status":"healthy"}
```

**Status Code:** 200 OK

## Rate Limiting

| Endpoint | Limit | Window | HTTP Code on Excess |
|----------|-------|--------|-------------------|
| POST `/Document/Create` | 5 requests | 1 minute per IP | 429 Too Many Requests |
| GET `/r/{token}` | 30 requests | 1 minute per IP | 429 Too Many Requests |
| GET `/r/{token}/pdf` | 30 requests | 1 minute per IP | 429 Too Many Requests |

## Error Responses

### 404 Not Found

Returned when an invalid `token` is provided.

### 429 Too Many Requests

Returned when rate limit is exceeded.

```json
// Default ASP.NET Core rate limiter response
Status: 429 Too Many Requests
```

### 500 Internal Server Error

In production, the generic `/Home/Error` page is shown. In development, detailed error information is returned.

## SMS Message Format

When a receipt is created, the following SMS message is dispatched:

```
Your {DocumentType} is ready.

DocLink
{publicUrl}
```

Example:

```
Your Receipt is ready.

DocLink
https://acomit-doclink-poc-production.up.railway.app/r/aB3xK9mP2Q
```

Currently dispatched via `MockSmsService` (logs to console only).

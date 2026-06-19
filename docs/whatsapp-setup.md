# WhatsApp Cloud API Setup Guide

## Overview

DocLink uses the **Meta WhatsApp Cloud API** to send document notifications via WhatsApp. This guide walks through setting up Meta's free test tier and configuring the application.

---

## Why it's free

- Meta provides a **test phone number** that can message **up to 5 verified phone numbers** — no charge
- **Utility** messages (order confirmations, receipts, account updates) within the 24h service window are **free**
- No credit card required for the test tier

---

## Prerequisites

- A Facebook personal account
- A phone number that can receive SMS (for verification)
- The recipient's phone number (for testing)

---

## Step 1 — Create Meta Developer Account

1. Go to https://developers.facebook.com
2. Click **Get Started** → register with your Facebook account
3. Complete registration at https://developers.facebook.com/async/registration/

## Step 2 — Create a Meta Business Portfolio

If you don't already have one:

1. Go to https://business.facebook.com
2. Click **Create Account**
3. Enter business name, your name, and email

## Step 3 — Create a Meta App with WhatsApp

1. Go to https://developers.facebook.com/apps → **Create App**
2. Select the **Connect with customers through WhatsApp** use case
3. Name it (e.g. "DocLink"), select your business portfolio
4. Click **Create App**

You are redirected to the WhatsApp API Setup page.

## Step 4 — Get Credentials

On the **WhatsApp → API Setup** page, copy these values:

| What to copy | Where it appears |
|---|---|
| **Phone Number ID** | "API Setup" panel — starts with a number |
| **Temporary Access Token** | Just below the phone number ID (valid 24 hours) |
| **Test Phone Number** | The "From" number (usually starts with `+1`) |

Save these — you'll set them as Railway environment variables.

## Step 5 — Add Recipient Phone Numbers

1. In **API Setup** → **Send and receive messages** → **To** field
2. Click **Manage phone number list**
3. Add up to 5 phone numbers (including the founder's)
4. Each receives a **verification code** via WhatsApp — enter it to confirm

## Step 6 — Send a Test Message

1. In the **API Setup** page, select a recipient
2. Click **Send message** — this sends the pre-approved `hello_world` template
3. If the recipient receives it, the API is working

## Step 7 — Open the 24h Service Window (for demo)

Meta requires a **pre-approved template** for the first business-initiated message to a new user. The simplest way around this:

1. Have the recipient **reply** to the test message with any text (e.g. "Hello")
2. This opens a **24-hour service window**
3. Within that window, DocLink can send **free-form text messages** — no template needed

For the founder demo, ask them to message the business number before the demo starts.

## Step 8 — Set Railway Environment Variables

```
WHATSAPP_ACCESS_TOKEN    = <temporary or permanent token>
WHATSAPP_PHONE_NUMBER_ID = <phone number ID from API Setup>
WHATSAPP_API_VERSION     = v22.0
```

The app is already configured to use `WhatsAppCloudApiService`. Once these env vars are set, it will send real WhatsApp messages instead of logging to the console.

---

## Production: Permanent Token

The temporary token expires every 24 hours. For a permanent token:

1. Go to https://business.facebook.com → **Business Settings**
2. **Users** → **System Users** → **Add**
3. Name the user (e.g. "DocLink API"), grant **Admin** role
4. Click **Generate Token**
5. Select your app, check `whatsapp_business_messaging` and `whatsapp_business_management`
6. Copy the token — it never expires (revocable)

## Production: Custom Template

For **cold-initiated** messages (recipient hasn't messaged you first), you need an approved template:

1. Go to https://business.facebook.com → **WhatsApp Manager** → **Message Templates**
2. Click **Create Template**
3. Category: **Utility** (free messages)
4. Name: `document_notification`
5. Language: English
6. Body text: `Your {{1}} is ready.\nOpen it here: {{2}}`
7. Submit for approval (1–24 hours)
8. Once approved, set: `WHATSAPP_TEMPLATE_NAME=document_notification`

The app will use this template for the first message, with `{{1}}` = document type and `{{2}}` = document URL.

---

## How the Code Works

The `WhatsAppCloudApiService.SendAsync` method:

1. **Tries free-form text** — works if the 24h service window is open
2. **If text is rejected** (template required), logs a suggestion and falls back to a template
3. **Sends a template message** — uses `WHATSAPP_TEMPLATE_NAME` env var, or `hello_world` default
4. **If template also fails** — logs a clear error message with resolution steps

No changes to `DocumentController` or other code are needed.

---

## Costs (Production)

| Message Type | Cost |
|---|---|
| **Utility** (receipts, order confirmations) | Free within 24h window |
| **Marketing** | Per-message pricing (varies by region) |
| **Authentication** (OTP) | Free |
| **Service** (user-initiated replies) | Free |

Meta charges per conversation, not per message. For DocLink's receipt notifications, all messages qualify as **Utility** — the cheapest tier.

---

## Troubleshooting

| Error | Likely Cause | Fix |
|---|---|---|
| `131000` / `131008` | Template required / not approved | Use an approved template name or have recipient message first |
| `131005` | Access token invalid/expired | Regenerate token or use a permanent System User token |
| `132000` | Template in review | Wait for approval or use `hello_world` for testing |
| `131026` | Number not in allowed list | Add recipient number in API Setup → Manage phone number list |
| Message not delivered | Recipient doesn't have WhatsApp | Check the phone number has WhatsApp installed |

---

## Local Sri Lanka Alternatives

If Meta Cloud API is unsuitable (e.g. display name approval issues, regional restrictions):

| Provider | Notes |
|---|---|
| **Bloomwire** | Sri Lanka-based WhatsApp Business Solution Provider |
| **Go4whatsup** | Supports Sinhala/Tamil, local support |
| **Notify.lk** | Sri Lanka SMS/WhatsApp provider |
| **Twilio WhatsApp** | Global, $0.005–0.05 per message |

See [messaging-providers.md](messaging-providers.md) for a full comparison.

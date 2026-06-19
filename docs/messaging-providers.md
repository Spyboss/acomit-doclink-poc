# Messaging Provider Guide — WhatsApp

DocLink sends receipt links to customers via WhatsApp. The app uses `IMessagingService` with a mock implementation by default. This guide covers replacing it with a real provider.

## Architecture

```
DocLink ──► IMessagingService.SendAsync(phone, message)
                │
                ├── MockMessagingService (logs to console, default)
                │
                └── WhatsAppCloudApiService (Meta Cloud API)
                        │
                        └── POST https://graph.facebook.com/v22.0/{id}/messages
```

## Meta WhatsApp Cloud API

The official API hosted by Meta. No middleman, directly from Meta.

### Prerequisites

1. **Meta Business Account** — [business.facebook.com](https://business.facebook.com)
2. **WhatsApp Business Account (WABA)** — created during onboarding
3. **Business verification** — submit documents (takes 1–3 business days)
4. **Phone number** — not registered with regular WhatsApp

### Setup

1. Go to [developers.facebook.com](https://developers.facebook.com)
2. Create an app → **Business** type
3. Add **WhatsApp** product
4. Go to **API Setup** — copy your **Phone Number ID** and **Access Token**
5. To receive messages, configure a **Webhook** endpoint at `{your-url}/webhooks/whatsapp`

### Pricing (2026)

| Category | Cost |
|----------|------|
| **Utility** (transactional: confirmations, receipts) | Free — no charge within 24h service window |
| **Marketing** (promotions, campaigns) | Varies by country ($0.01–$0.08 per conversation) |
| **Authentication** (OTP) | $0.01–$0.05 per conversation |
| **Service** (customer support) | Free within 24h of last user message |
| **WhatsApp Phone Number** | Free |
| **Business verification** | Free |
| **Template pre-approval** | Free (Meta reviews templates) |

> **Sri Lanka pricing (2026):** Marketing conversations to Sri Lanka are ~$0.03–$0.06 per conversation. Utility conversations (receipt notifications) are **free** in the 24h service window.

### Rate Limits

| Limit | Value |
|-------|-------|
| Messages per second | 80 msg/sec |
| Template sends per day | 250 per phone number |
| Business-initiated conversations | Must use pre-approved templates |
| User-initiated conversations | Free-form text allowed (24h window) |

### Code Implementation

The `WhatsAppCloudApiService` is already in the project:

```csharp
// In Program.cs, swap to use it:
// builder.Services.AddScoped<IMessagingService, MockMessagingService>();
builder.Services.AddScoped<IMessagingService, WhatsAppCloudApiService>();
```

Set these environment variables:

| Variable | Description |
|----------|-------------|
| `WHATSAPP_ACCESS_TOKEN` | Permanent access token from Meta Developer Console |
| `WHATSAPP_PHONE_NUMBER_ID` | Phone number ID (numeric, from API Setup page) |
| `WHATSAPP_API_VERSION` | Optional, defaults to `v22.0` |

### Template Messages

For business-initiated messages (first message to a customer), you **must** use a pre-approved template. Create one in the Meta Developer Console:

1. Go to **WhatsApp → Template Management**
2. Click **Create Template** → **Utility** category
3. Template body: `Your {{1}} is ready.\n\nDocLink\n{{2}}`
4. Submit for review (typically approved within 24 hours)

Then send via the API:

```json
{
  "messaging_product": "whatsapp",
  "to": "9471XXXXXXX",
  "type": "template",
  "template": {
    "name": "receipt_ready",
    "language": { "code": "en" },
    "components": [{
      "type": "body",
      "parameters": [
        { "type": "text", "text": "Receipt" },
        { "type": "text", "text": "https://yourdomain.com/r/abc123" }
      ]
    }]
  }
}
```

> **Note:** For this PoC, receipts are sent within a customer-initiated 24h window, so free-form text messages work without templates.

---

## Sri Lankan WhatsApp Providers

Several local providers offer WhatsApp Business API access with simpler onboarding:

| Provider | Monthly Fee | Per-Message | Onboarding | Features |
|----------|------------|-------------|------------|----------|
| **Bloomwire** | From LKR 20,000 (~$65) | LKR 0.50–2 | Fast, local support | SMS + WhatsApp + AI chatbot in one |
| **Go4whatsup** | $49–$199/mo | Meta rates passed through | Same-day | Sinhala/Tamil auto-translation, PDPA compliant |
| **Notify.lk** | Contact | Contact | Standard | SMS + WhatsApp, campaign manager |
| **CloudCoder** | Custom (~LKR 50K setup) | LKR 0.50–2 | Managed | Full WhatsApp automation, bot building |
| **Text.lk** | Free tier + pay-as-you-go | Contact | Fast | SMS + WhatsApp integration |
| **Unecast/Bloomwire** | Custom | LKR 0.50–2 | 1-2 days | AI chatbot + WhatsApp API + SMS |

### Bloomwire (Recommended for Sri Lanka)

- **Meta Business Partner** — official WhatsApp API access
- **Platform:** WhatsApp + SMS + AI Live Chat in one dashboard
- **Languages:** Sinhala, Tamil, English
- **Onboarding:** Local support team in Sri Lanka
- **Trust:** 150+ businesses, 10M+ messages processed
- **Pricing:** Transparent, no hidden fees

### Go4whatsup

- Strong Sinhala/Tamil support with auto-translation
- PDPA (Sri Lanka Data Protection Act) compliant
- Same-day onboarding with India office (UTC+5:30)
- Native integrations with Shopify, Odoo, Zoho CRM

---

## International Alternatives

| Provider | Pricing | Type | Notes |
|----------|---------|------|-------|
| **Twilio WhatsApp API** | $0.005/msg + Meta fees | BSP (Business Solution Provider) | Best docs, requires WABA setup |
| **MessageBird (Bird)** | $0.007/msg | BSP | Omnichannel, strong in Europe |
| **Vonage** | $0.0076/msg | BSP | Enterprise focus, voice + WhatsApp |
| **WATI** | $49–$199/mo | SaaS platform | No-code, shared inbox, templates |
| **Interakt** | ₹2,999–₹9,999/mo | SaaS platform | India-focused, affordable |
| **SendZen** | $29–$99/mo | SaaS platform | Simple API, webhook support |

### Twilio WhatsApp

- Twilio acts as a Business Solution Provider (BSP) for WhatsApp
- Pricing: $0.005/Twilio platform fee + Meta conversation fees
- Total per conversation: $0.01–$0.08 depending on category and country
- Rate limits: 80 msg/sec
- Requires: Meta Business Account, WABA, business verification
- **Pros:** Excellent documentation, existing Twilio accounts can reuse credentials
- **Cons:** Hidden costs stack up; Sri Lanka messages may have higher Meta fees

---

## Implementation in DocLink

### Switching Providers

In `Program.cs`, change one line:

```csharp
// Default (mock - logs to console):
builder.Services.AddScoped<IMessagingService, MockMessagingService>();

// Meta Cloud API (direct):
// builder.Services.AddScoped<IMessagingService, WhatsAppCloudApiService>();

// Local LK provider - create your own:
// builder.Services.AddScoped<IMessagingService, BloomwireMessagingService>();
```

### Creating a Custom Provider

Implement `IMessagingService`:

```csharp
public class BloomwireMessagingService : IMessagingService
{
    private readonly HttpClient _http;
    public BloomwireMessagingService(HttpClient http) => _http = http;

    public async Task SendAsync(string phoneNumber, string message)
    {
        var payload = new { to = phoneNumber, text = message };
        _http.DefaultRequestHeaders.Add("API-Key", Environment.GetEnvironmentVariable("BLOOMWIRE_API_KEY"));
        var response = await _http.PostAsJsonAsync("https://api.bloomwire.lk/whatsapp/send", payload);
        response.EnsureSuccessStatusCode();
    }
}
```

## Comparison: WhatsApp vs SMS for DocLink

| Factor | SMS | WhatsApp |
|--------|-----|----------|
| **Cost to Sri Lanka** | $0.42 (Twilio) / 0.64 LKR (local) | Free (utility) or ~$0.03–0.06 (marketing) |
| **Open rate** | ~98% | ~98% |
| **Rich media** | Text only (160 chars) | Text, images, buttons, links |
| **Delivery** | Carrier-dependent | Internet-dependent (WiFi/mobile data) |
| **Template required** | No | Yes (for first business message) |
| **24h window** | No | Yes (free-form after user messages) |
| **Opt-in required** | Generally no | Yes (user must initiate or opt in) |

**Recommendation for DocLink:** Use **Meta Cloud API directly** (free for utility messages) since receipt notifications qualify as transactional/utility conversations. For local support, **Bloomwire** or **Go4whatsup** provide easier onboarding with Sinhala/Tamil support.

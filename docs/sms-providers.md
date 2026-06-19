# SMS Provider Comparison

The app currently uses `MockSmsService`, which only logs messages to the console. To send real SMS, implement `ISmsService` with one of the providers below.

## Provider Comparison Summary

### International Providers

| Provider | US SMS Price | Sri Lanka SMS Price | Rate Limit (per long code) | Phone Number (monthly) | Notes |
|----------|-------------|--------------------|--------------------------|----------------------|-------|
| **Twilio** | $0.0083/msg | $0.42/msg | 1 msg/sec | $1.15 | Best docs, most APIs, hidden carrier fees |
| **Plivo** | ~$0.005/msg | — | 1 msg/sec | ~$1.00 | 30-40% cheaper than Twilio, clean API |
| **Vonage** (Nexmo) | ~$0.0076/msg | — | — | ~$0.85 | Owned by Ericsson, good voice + SMS |
| **MessageBird** (Bird) | ~$0.007/msg | — | — | ~$1.00 | Strong in Europe/Asia, omnichannel |
| **AWS SNS** | ~$0.006/msg | varies | — | — | Cheapest if already on AWS, no dashboard |

### Local Sri Lankan Providers

| Provider | Price (LKR/SMS) | Price (USD/SMS) | Trial | API | Notes |
|----------|----------------|----------------|-------|-----|-------|
| **Text.lk** | 0.64 LKR | ~$0.002 | Free credits | REST API | Cheapest, Sender ID support |
| **Notify.lk** | Contact for pricing | — | Free credits | REST API | Dashboard + campaign manager |
| **FITSMS** | 0.68 LKR | ~$0.002 | Start for free | REST API | No setup fees, no expiry credits |
| **SMSlenz** | Contact for pricing | — | Free trial | REST API | All local networks (Dialog, Mobitel, Hutch, Airtel) |
| **Textit.biz** | Market lowest | — | — | REST API | Since 2011, own SMS servers |
| **TxtMsg.lk** | Contact for pricing | — | Free trial | REST API | 99.9% delivery rate, TRCSL compliant |

> **Note:** Twilio charges **$0.42/msg** to Sri Lanka. Local providers charge **~$0.002/msg** — a **200x cost difference** for local SMS. If your recipients are in Sri Lanka, use a local provider.

---

## Twilio — Detailed Breakdown

### Pricing (2026)

| Item | Cost |
|------|------|
| US/Canada SMS (outbound, long code) | $0.0079–$0.0083 per segment |
| US MMS (outbound) | $0.022 per segment |
| **Sri Lanka SMS (outbound)** | **$0.42 per segment** |
| US Toll-Free SMS | $0.0083 + carrier surcharges |
| US Short Code rental | $1,000–$1,500/month |
| Phone number (long code) | $1.15/month |
| Toll-Free number | $2.15/month |
| 10DLC brand registration | $4.50 one-time |
| 10DLC campaign fee | $1.50–$10/month |
| Carrier surcharges (US) | $0.003–$0.005 per msg (T-Mobile, Verizon, ATT) |
| Free trial | $15 credit |

Hidden costs: Carrier surcharges add **68–150%** to base price. A $0.0083 msg can cost **$0.012–$0.015** all-in.

### Rate Limits

| Number Type | Messages per Second (MPS) | Messages per Hour (approx) |
|-------------|--------------------------|---------------------------|
| Long code (1 number) | 1 msg/sec | 3,600 |
| Toll-Free | 3 msg/sec (up to 25 with HT TF) | 10,800 |
| Short code | 100 msg/sec | 360,000 |
| WhatsApp | 80 msg/sec | 288,000 |

For higher throughput: buy additional numbers (long code) or contact sales (short code, toll-free).

### Geo Permissions

By default, new Twilio accounts can only send to their home country. To send to **Sri Lanka**:

1. Go to **Console > Messaging > Settings > Geo Permissions**
2. Find **Sri Lanka** and enable it
3. Save changes

### Limitations

- **Trial accounts** can only send to verified numbers, with a "Sent from Twilio trial" prefix
- **Message segments** — SMS >160 chars (GSM-7) or >70 chars (Unicode) split into segments, each segment billed separately
- **A2P 10DLC** required for US application-to-person messaging (brand + campaign registration)
- **No SLA** for delivery speed — Twilio does not guarantee throughput

### Implementation in DocLink

```csharp
// 1. Install: dotnet add package Twilio

// 2. Create TwilioSmsService.cs
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace DocLink.Services;

public class TwilioSmsService : ISmsService
{
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(ILogger<TwilioSmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        var fromNumber = Environment.GetEnvironmentVariable("TWILIO_FROM_NUMBER");

        TwilioClient.Init(accountSid, authToken);

        var msg = await MessageResource.CreateAsync(
            body: message,
            from: new Twilio.Types.PhoneNumber(fromNumber),
            to: new Twilio.Types.PhoneNumber(phoneNumber)
        );

        _logger.LogInformation("SMS sent: {Sid}, Status: {Status}", msg.Sid, msg.Status);
    }
}

// 3. In Program.cs, swap:
// builder.Services.AddScoped<ISmsService, MockSmsService>();
builder.Services.AddScoped<ISmsService, TwilioSmsService>();

// 4. Set env vars:
// TWILIO_ACCOUNT_SID=your_account_sid
// TWILIO_AUTH_TOKEN=your_auth_token
// TWILIO_FROM_NUMBER=+1XXXXXXX
```

---

## Local Sri Lankan Providers

### Text.lk (Recommended for Sri Lanka)

- **Price:** 0.64 LKR per SMS (~$0.002)
- **API:** REST API with libraries for PHP, Java, Laravel, WordPress, Shopify
- **Features:** Sender ID, bulk SMS, delivery reports, two-way messaging
- **Trial:** Free credits on signup
- **Coverage:** All Sri Lankan networks (Dialog, Mobitel, Hutch, Airtel, SLT)

### Notify.lk

- **Price:** Contact for pricing
- **API:** REST API with developer docs at [developer.notify.lk](https://developer.notify.lk)
- **Features:** Campaign manager, customer manager, sender ID, integrations (WooCommerce, Zapier)
- **Trial:** Free credits on signup
- **Support:** Phone 0766-4444-65

### FITSMS

- **Price:** 0.68 LKR per SMS (all inclusive, no hidden fees)
- **API:** REST API with WooCommerce/Shopify plugins
- **Features:** Unicode (Sinhala/Tamil) support, 160 chars English / 70 chars Unicode per segment, no monthly fees
- **Trial:** Free trial available

### SMSlenz

- **Price:** Contact for pricing
- **API:** REST API with developer docs
- **Features:** Dashboard, advanced reports, subscription plans, 24/7 support
- **Coverage:** Dialog, Mobitel, Hutch, Airtel, SLT
- **Trial:** Free trial available

### Textit.biz

- **Price:** Claims lowest market rates
- **Operation:** Since 2011, own in-house SMS servers
- **Features:** No setup fees, no monthly fees, no hidden costs

### TxtMsg.lk

- **Price:** Contact for pricing
- **Features:** Dynamic Sender ID, OTP, campaign manager, 99.9% delivery rate
- **Compliance:** 100% TRCSL compliant
- **Trial:** Free trial available

---

## International Alternatives

### Plivo

- **US SMS:** ~$0.005/msg (30–40% cheaper than Twilio)
- **API:** Clean REST API, good documentation
- **Pros:** Lower price, solid core SMS/voice
- **Cons:** Fewer add-ons than Twilio

### Vonage (Nexmo)

- **US SMS:** ~$0.0076/msg
- **API:** Programmable SMS + Voice + Verify
- **Pros:** Competitive high-volume pricing, strong voice APIs
- **Cons:** Less developer-friendly than Twilio

### MessageBird (Bird)

- **US SMS:** ~$0.007/msg
- **API:** Omnichannel (SMS, WhatsApp, email)
- **Pros:** Strong in Europe and Asia, competitive international rates
- **Cons:** Platform less mature than Twilio

### AWS SNS

- **US SMS:** ~$0.006/msg
- **API:** AWS SDK integration
- **Pros:** Cheapest if already on AWS, no minimums
- **Cons:** No dashboard, limited features, no two-way messaging

---

## Recommendation for DocLink

| Scenario | Recommended Provider | Est. Cost (1,000 SMS/mo) |
|----------|--------------------|------------------------|
| **Sri Lanka recipients** | Text.lk or FITSMS | ~$2 (640–680 LKR) |
| **Global recipients** | Twilio (best DX) | ~$8–$15 |
| **Global recipients (cheapest)** | Plivo | ~$5 |
| **Already on AWS** | AWS SNS | ~$6 |

For this PoC serving Sri Lankan customers, **Text.lk** is the most cost-effective choice at ~$0.002/msg vs Twilio's $0.42/msg.

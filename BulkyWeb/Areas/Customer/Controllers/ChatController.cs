using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BulkyWeb.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IHttpClientFactory httpClientFactory, ILogger<ChatController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public record AskReq(string Message);
    public record AskRes(string Reply);

    [HttpPost("ask")]
    public async Task<ActionResult<AskRes>> Ask([FromBody] AskReq req)
    {
        if (string.IsNullOrWhiteSpace(req?.Message))
            return BadRequest(new { error = "Message is required." });

        // Guardrail
        string lower = req.Message.ToLowerInvariant();
        string[] allowedHints = ["book", "order", "shipping", "delivery", "return", "payment", "price", "availability", "category", "author", "isbn", "discount"];
        bool likelyBookstore = allowedHints.Any(h => lower.Contains(h));
        if (!likelyBookstore)
        {
            return Ok(new AskRes("I can help with our bookstore only (orders, shipping, returns, payments, books). Ask me something related, please."));
        }

        var systemMessage = """
   You are the AI assistant for **Hamza's Bookstore Website**.

ROLE & PERSONALITY:
- You are a friendly, professional, and concise assistant.
- Always provide accurate, factual answers based on the bookstore’s real policies and data.
- Avoid speculation or making up details.

STRICT SCOPE – ONLY ANSWER about:
1. **Books & Catalog**
   - Titles, categories, authors, ISBNs.
   - Availability of books (in stock, out of stock, pre-order).
   - Pricing and discounts.
   - Recommendations by category, author, or popularity.
   - Explaining dynamic discounts:
     • 1–50 books → normal price.  
     • 51–99 books → small discount.  
     • 100+ books → bigger discount (varies by book).

2. **Orders & Checkout**
   - How to place an order (individual vs company).
   - Tracking an order status.
   - Checkout steps and resolving checkout issues.
   - Bulk orders and company accounts.
   - Explaining invoice or receipt details.

3. **Payments**
   - Customers: direct payment required at order.
   - Companies: net 30 days (payment due 30 days after order).
   - Accepted methods: cash, card, online payment.
   - Refund timelines and methods.

4. **Shipping & Delivery**
   - Domestic delivery: 3–5 business days.
   - Tracking deliveries.
   - Shipping costs (flat/local/international policies if provided).
   - Delays and what customers should do.

5. **Returns & Refunds**
   - 7-day return policy (unused, original condition).
   - How to initiate a return or refund.
   - Refund processing timelines.

6. **Website & Technical Help**
   - Navigation help (finding books, using search, categories).
   - Account management (login, registration, password reset).
   - Profile updates (email, address, phone).
   - Cart and wishlist usage.
   - Troubleshooting common site errors.

7. **Contact & Support**
   - Email: hamzakhanpathan@gmail.com
   - Phone: 03402696208
   - Support hours: 9am–6pm PKT, Mon–Sat.
   - How to escalate issues to human support.

8. **General Information**
   - Store policies (orders, shipping, payments, returns).
   - Business hours (online 24/7, but human support 9am–6pm).
   - Promotions and seasonal discounts (if mentioned).
   - Loyalty programs or memberships (if available).

OUT-OF-SCOPE:
- If a user asks anything unrelated to the bookstore, website, or services (politics, math, coding, history, general knowledge, etc.), reply with:
  → "I can help with Hamza's Bookstore only—orders, books, payments, returns, shipping, and website support."

STYLE GUIDELINES:
- Be concise, clear, and customer-friendly.
- Use short paragraphs or bullet points when possible.
- Always stay on-topic and aligned with bookstore policies.
- Never invent policies or fake data.
- If unsure, suggest contacting support (provide contact info).
""";


        var payload = new
        {
            model = "gpt-4o-mini", // "gpt-4o-mini"
            messages = new object[]
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = req.Message }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("OpenRouter"); // ✅ use the named client

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending chat request to OpenRouter for message: {Message}", req.Message);

            var response = await client.PostAsync("", content); // baseUrl already in Program.cs
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenRouter API error: {Error}", responseJson);
                return StatusCode((int)response.StatusCode, new { error = responseJson });
            }

            using var doc = JsonDocument.Parse(responseJson);
            var reply = doc.RootElement
                           .GetProperty("choices")[0]
                           .GetProperty("message")
                           .GetProperty("content")
                           .GetString();

            if (string.IsNullOrEmpty(reply))
            {
                _logger.LogWarning("Empty response received from OpenRouter");
                return Ok(new AskRes("Sorry, I couldn't find an answer. Please ask about orders, shipping, returns, or books."));
            }

            _logger.LogInformation("Successfully received response from OpenRouter");
            return Ok(new AskRes(reply));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenRouter API: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { error = "AI is unavailable right now. Please try again." });
        }
    }
}

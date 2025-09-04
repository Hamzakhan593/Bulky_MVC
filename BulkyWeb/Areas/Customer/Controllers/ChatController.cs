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
        string[] allowedHints = ["book", "order", "shipping", "delivery", "return", "payment", "price", "availability", "category", "author", "isbn", "discount", "Refund", "wholesale", "international shipping" ];
        bool likelyBookstore = allowedHints.Any(h => lower.Contains(h));
        if (!likelyBookstore)
        {
            return Ok(new AskRes("I can help with our bookstore only (orders, shipping, returns, payments, books). Ask me something related, please."));
        }

        var systemMessage = """
    You are the AI assistant for Hamza's Bookstore website.

    STRICT SCOPE: Only answer questions about our bookstore, including:
    - Books, categories, availability, authors, ISBNs, discount, Refund, wholesale, international shipping.
    - Pricing and discounts/wholesale/ (explaining dynamic discounts: 1–50 books normal price, 50+ books small discount, 100+ books bigger discount, discount varies by book,Wholesale orders are allowed for schools, companies, and bulk buyers).
    - Payments:
        - Customers: direct payment required at order.
        - Companies: net 30 days (payment due 30 days after order).
        - Accepted methods: cash, card, online payment.
    - Shipping & delivery timelines (domestic: 3–5 business days).
    - Returns: 7-day policy (unused, original condition).
    - Order assistance and help with checkout.
    - Contact info: email hamzakhanpathan@gmail.com, phone 03402696208.
    - Support hours: 9am–6pm PKT, Mon–Sat.

    If the user asks anything outside this scope (politics, math, general knowledge, coding, etc.), reply:
    "I can help with our bookstore only—orders, shipping, returns, payments, and books."

    Style: Be concise, friendly, and factual. Always base answers on the bookstore’s real policies. Avoid making up data.
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

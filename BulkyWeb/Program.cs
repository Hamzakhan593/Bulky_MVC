using Bulky.DataAccess.DbInitializer;
using Bulky.DataAccess.Resository;
using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using BulkyWeb.DataAccess;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Stripe;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Services ------------------- //
builder.Services.AddControllersWithViews();

// DB Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection")));

// Stripe Config
builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(100);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

// DI for your app services
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddRazorPages();

// ------------------- OpenAI Chatbot Config ------------------- //
// Read API Key from config (appsettings.json or Azure AppSettings)
// Read config
string? apiKey = builder.Configuration["OpenRouter:ApiKey"];
string? baseUrl = builder.Configuration["OpenRouter:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(baseUrl))
{
    Console.WriteLine("WARNING: OpenRouter API key or BaseUrl not found. Chat functionality will be disabled.");
    builder.Services.AddSingleton<HttpClient>(_ => null!);
}
else
{
    // Register HttpClient for OpenRouter
    builder.Services.AddHttpClient("OpenRouter", client =>
    {
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000"); // required by OpenRouter
        client.DefaultRequestHeaders.Add("X-Title", "Hamza Bookstore Bot");        // optional, app name
    });
}

// ------------------- Build App ------------------- //
var app = builder.Build();

// ------------------- Middleware ------------------- //
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// DB Seeder
SeedDatabase();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();

// ------------------- DB Seed Method ------------------- //
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}